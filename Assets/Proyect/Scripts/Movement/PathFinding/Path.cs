using Burmuruk.AI;
using Burmuruk.Tesis.Utilities;
using Burmuruk.WorldG.Patrol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using static UnityEditor.Progress;

namespace Burmuruk.Tesis.Movement.PathFindig
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Path", order = 2)]
    public class Path : ScriptableObject
    {
        [SerializeField] bool saved;
        [SerializeField] float radious;
        [SerializeField] float distance;
        [SerializeField] float maxAngle;
        [SerializeField] List<int> towerSizes;
        [SerializeField] List<int> towerHeights;
        [SerializeField] ScrNode[] towers;

        Dictionary<uint, Queue<(int idx, int connectionIdx)>> nodesWaiting;
        Dictionary<uint, int> addedNodes;
        [SerializeField] DictionaryNodeString[] nodesWaitingSaved;
        [SerializeField] DictionaryNodeInt[] addedNodesSaved;

        const string fileName = "NavGrid";
        private bool working = false;
        public NodeListSuplier NodeList { get; private set; }

        private struct IdData
        {
            public uint Id;
            
            public int idx;
            public int ConnectionIdx;
        }

        public bool Saved { get => saved; }
        public bool Loaded { get; private set; }

        public void SaveList(IPathNode[][][] nodes, int count)
        {
            Stopwatch st = Stopwatch.StartNew();
            st.Start();
            LinkedList<string> text = new LinkedList<string>();
            text.AddFirst(nodes.Length.ToString());

            for (int i = 0; i < nodes.Length; ++i)
            {
                //text += nodes.Length + ">";
                //text += nodes[i][0].Length + ">" + nodes[i][0][0].Position.y + ">";
                text.AddLast(string.Concat(nodes[i][0].Length, ">", nodes[i][0][0].Position.y, ">"));

                for (int j = 0; j < nodes[i].Length; ++j)
                {
                    text.AddLast(nodes[i].Length + ">" + nodes[i][j][0].Position.z);

                    for (int k = 0; k < nodes[i][j].Length; ++k)
                    {
                        //ref var cur = ref nodes[i][j][k];

                        //text += $"{cur.ID}*{cur.Position.x},{cur.Position.y},{cur.Position.z}{GetNodeConnections(cur)}";
                        text.AddLast(string.Concat(nodes[i][j][k].ID, "*", nodes[i][j][k].Position.x, GetNodeConnections(nodes[i][j][k])));
                    }
                }

                text.AddLast("\\");
            }

            Write(text);


            saved = true;
            st.Stop();

            UnityEngine.Debug.Log("Saved in: " + st.Elapsed.Milliseconds);
        }

        private void Write(LinkedList<string> text)
        {
            string path = UnityEngine.Application.persistentDataPath + fileName + SceneManager.GetActiveScene().buildIndex + ".txt";

            using (Stream stream = new FileStream(path, FileMode.Create))
            {
                foreach (var item in text)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(item);
                    stream.Write(bytes, 0, item.Length); 
                }
            }
        }

        private void SaveConnections()
        {
            addedNodesSaved = Serialize(addedNodes);
            nodesWaitingSaved = Serialize(nodesWaiting);

            addedNodes.Clear();
            nodesWaiting.Clear();
        }

        private void UpdateIDs()
        {
            foreach (var list in nodesWaiting)
            {
                while(list.Value.Count > 0)
                {
                    var ids = list.Value.Dequeue();
                    var lastConnection = towers[ids.idx].NodeConnections[ids.connectionIdx];
                    lastConnection.node = towers[addedNodes[list.Key]];

                    towers[ids.idx].NodeConnections[ids.connectionIdx] = lastConnection;
                }
            }
        }

        private void InitVariables()
        {
            nodesWaiting = new Dictionary<uint, Queue<(int idx, int connectionIdx)>> ();
            addedNodes = new Dictionary<uint, int>();
            towerSizes = new List<int>();
            towerHeights = new List<int>();
            Loaded = false;
            working = false;
        }

        private string GetNodeConnections(IPathNode node)
        {
            string connections = "";

            foreach (var cnc in node.NodeConnections)
            {
                connections += $"-{cnc.node.ID}";
            }

            return connections += ")";
        }

        private void SetNodeConnections(in IPathNode curNode, ref ScrNode newNode, in int nodeIdx)
        {
            var connections = new List<NodeConnection>();
            addedNodes.Add(newNode.ID, nodeIdx);
            uint id = 0;

            for (int i = 0; i < curNode.NodeConnections.Count; i++)
            {
                id = curNode.NodeConnections[i].node.ID;

                //if (addedNodes.ContainsKey(id))
                //{
                //    connections.Add(new NodeConnection((ScrNode)towers[addedNodes[id]], curNode.NodeConnections[i].connectionType, curNode.NodeConnections[i] .Magnitude));
                //}
                //else
                connections.Add(new NodeConnection(null, curNode.NodeConnections[i].connectionType, curNode.NodeConnections[i].Magnitude));
                        
                if (nodesWaiting.ContainsKey(id))
                {
                    nodesWaiting[id].Enqueue((nodeIdx, connections.Count - 1));
                }
                else
                {
                    var list = new Queue<(int, int)>();
                    list.Enqueue((nodeIdx, connections.Count - 1));

                    nodesWaiting.Add(id, list);
                }
            }

            newNode.SetConnections(connections);
        }

        public void SaveExtraData(float radious, float distance, float maxAngle)
        {
            this.distance = distance;
            this.maxAngle = maxAngle;
            this.radious = radious;

            EditorUtility.SetDirty(this);
        }

        private void SetNodeList(IPathNode[][][] nodes)
        {
            NodeList = new NodeListSuplier(nodes);

            NodeList.SetTarget(radious, distance, maxAngle);
        }

        public void LoadNavMesh()
        {
            if (Loaded || working) return;

            working = true;

            SetNodeList(GetReversedList());

            Loaded = true;
            working = false;
            //Task<IPathNode[][][]> loadDataTask = Task.Run(() => GetReversedList());
            //loadDataTask.ContinueWith((antecedent) =>
            //{
            //    SetNodeList(antecedent.Result);
                
            //    Loaded = true;
            //    working = false;
            //}, TaskContinuationOptions.ExecuteSynchronously);
        }
        
        private IPathNode[][][] GetReversedList()
        {
            var nodes = new IPathNode[towerSizes.Count][][];
            int yidx = 0;
            int idx = 0;

            LinkedList<char[]> text = LoadFromFile();
            ReadString(text);
            RestartDictionaries();
            UpdateIDs();

            for (int i = 0; i < towerSizes.Count; i++)
            {
                nodes[i] = new IPathNode[towerSizes[i]][];

                for (int j = 0; j < towerSizes[i]; j++)
                {
                    IPathNode[] zNodes = new IPathNode[towerHeights[yidx]];

                    for (int k = 0; k < towerHeights[yidx]; k++)
                    {
                        zNodes[k] = towers[idx++];
                    }

                    nodes[i][j] = zNodes;
                    yidx++;
                }
            }

            return nodes;
        }

        private void ReadString(LinkedList<char[]> text)
        {
            IPathNode[][][] nodes = null;
            List<float> position = new();
            Queue<uint> ids = new();
            string number = "";
            List<int> sizes = new();

            foreach (var item in text)
            {
                for (int i = 0; i < item.Length; i++)
                {
                    switch (item[i])
                    {
                        case '>':
                            Int32.TryParse(number, out int newInt);

                            sizes.Add(newInt);
                            number = "";
                            break;

                        case '*':
                            uint.TryParse(number, out uint newId);

                            ids.Enqueue(newId);
                            number = "";
                            break;

                        case ',':
                            float.TryParse(number, out float newFloat);

                            position.Add(newFloat);
                            number = "";
                            break;

                        case ')':
                            var newNode = new ScrNode(ids.Dequeue(), new Vector3(position[0], position[1], position[2]));
                            //IPathNode newNode = null;

                            //NodeConnection newCnc = new NodeConnection()
                            //newNode.SetConnections();
                            break;

                        case '-':
                        case '/':

                            break;

                        case '\\':

                            break;

                        default:
                            number += item[i];
                            break;
                    }

                    ++i;
                }
            }
        }

        private LinkedList<char[]> LoadFromFile()
        {
            string path = UnityEngine.Application.persistentDataPath + fileName + SceneManager.GetActiveScene().buildIndex + ".txt";
            var text = new LinkedList<char[]>();

            using (Stream stream = new FileStream(path, FileMode.Create))
            {
                int totalRedaded = 0;

                do
                {
                    byte[] bytes = null;
                    totalRedaded = stream.Read(bytes, 0, 50);
                    text.AddLast(Encoding.UTF8.GetChars(bytes));
                    
                } while(totalRedaded > 0);
            }

            return text;
        }

        private void RestartDictionaries()
        {
            addedNodes = new Dictionary<uint, int>();

            for (int i = 0; i < addedNodesSaved.Length; i++)
            {
                addedNodes.Add(addedNodesSaved[i].key, addedNodesSaved[i].idx);
            }

            nodesWaiting = new Dictionary<uint, Queue<(int idx, int connectionIdx)>>();

            for (int i = 0; i < nodesWaitingSaved.Length; i++)
            {
                var values = new Queue<(int idx, int connectionIdx)>();
                var tuples = new Span<string>(nodesWaitingSaved[i].idxs.Split('/','-'));

                for (int j = 0; j < tuples.Length - 1;)
                {
                    values.Enqueue((Int32.Parse(tuples[j++]), Int32.Parse(tuples[j++])));
                }

                nodesWaiting.Add(nodesWaitingSaved[i].key, values);
            }
        }

        public DictionaryNodeInt[] Serialize(Dictionary<uint, int> dictionary)
        {
            var array = new DictionaryNodeInt[dictionary.Count];
            int i = 0;

            foreach (var item in dictionary)
            {
                array[i] = new DictionaryNodeInt(item.Key, item.Value);
                i++;
            }

            return array;
        }

        public DictionaryNodeString[] Serialize(Dictionary<uint, Queue<(int idx, int connectionIdx)>> dictionary)
        {
            var array = new DictionaryNodeString[dictionary.Count];
            int i = 0;

            foreach (var item in dictionary)
            {
                string idxs = "";

                foreach (var value in item.Value)
                {
                    idxs += $"{value.idx}-{value.connectionIdx}/";
                }

                array[i] = new DictionaryNodeString(item.Key, idxs);
                i++;
            }

            return array;
        }

        [System.Serializable]
        public struct DictionaryNodeInt
        {
            [SerializeField]public uint key;
            [SerializeField] public int idx;

            public DictionaryNodeInt(uint key, int idx)
            {
                this.key = key;
                this.idx = idx;
            }
        }

        [System.Serializable]
        public struct DictionaryNodeString
        {
            [SerializeField]public uint key;
            [SerializeField] public string idxs;

            public DictionaryNodeString(uint key, string idx)
            {
                this.key = key;
                this.idxs = idx;
            }
        }
    }
}
