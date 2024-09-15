using Burmuruk.AI;
using Burmuruk.WorldG.Patrol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        Dictionary<uint, Queue<((int x, int y, int z) idx, int connectionIdx)>> nodesWaiting;
        Dictionary<uint, IPathNode> addedNodes;

        const string fileName = "NavGrid";
        [SerializeField] private bool working = false;
        [SerializeField] bool loaded;
        public NodeListSuplier NodeList { get; private set; }

        private struct IdData
        {
            public uint Id;
            
            public int idx;
            public int ConnectionIdx;
        }

        public bool Saved { get => saved; }
        public bool Loaded { get => loaded; private set => loaded = value; }

        public void SaveList(IPathNode[][][] nodes, int count)
        {
            LinkedList<string> text = new LinkedList<string>();
            text.AddFirst(nodes.Length.ToString() + ">" + nodes[0][0][0].Position.x.ToString() + "|");

            for (int i = 0; i < nodes.Length; ++i)
            {
                text.AddLast(string.Concat(nodes[i].Length, ">"));

                for (int j = 0; j < nodes[i].Length; ++j)
                {
                    text.AddLast(nodes[i][j].Length + ">");

                    for (int k = 0; k < nodes[i][j].Length; ++k)
                    {
                        text.AddLast(string.Concat(nodes[i][j][k].ID, "*", nodes[i][j][k].Position.y, "|", nodes[i][j][k].Position.z, "|", GetNodeConnections(nodes[i][j][k])));
                    }
                }

                text.AddLast("\\");
            }

            Write(text);
            saved = true;
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

        private string GetNodeConnections(IPathNode node)
        {
            string connections = "";

            foreach (var cnc in node.NodeConnections)
            {
                connections += cnc.node.ID + "*";
            }

            return connections += ")";
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
            if (/*Loaded ||*/ working) return;

            working = true;

            SetNodeList(GetReversedList());
            UnityEngine.Debug.Log("Nav loaded");
            //Loaded = true;
            //Task<IPathNode[][][]> loadDataTask = Task.Run(() => GetReversedList());
            //loadDataTask.ContinueWith((antecedent) =>
            //{
            //    SetNodeList(antecedent.Result);
                
            //    Loaded = true;
            working = false;
            //}, TaskContinuationOptions.ExecuteSynchronously);
        }
        
        private IPathNode[][][] GetReversedList()
        {
            addedNodes = new ();
            nodesWaiting = new ();

            LinkedList<char[]> text = LoadFromFile();
            GetNodesFromText(text, out IPathNode[][][] nodes);
            SetConnections(nodes);

            return nodes;
        }

        private void GetNodesFromText(LinkedList<char[]> text, out IPathNode[][][] nodes)
        {
            nodes = null;
            Queue<uint> ids = new();
            string number = "";
            int sizes = -1;
            int positions = -1;
            int x = -1, y = -2, z = -1;
            float xPos = 0, yPos = 0, zPos = 0;
            float initialXPos = 0;

            foreach (var item in text)
            {
                for (int i = 0; i < item.Length; ++i)
                {
                    switch (item[i])
                    {
                        case '>':
                            Int32.TryParse(number, out int newInt);

                            if (sizes > 0)
                            {
                                ++y;
                                nodes[x][y] = new IPathNode[newInt];
                                z = 0;
                            }
                            else if (sizes == 0)
                            {
                                nodes[x] = new IPathNode[newInt][];
                                ++sizes;
                                ++y;
                            }
                            else
                            {
                                nodes = new IPathNode[newInt][][];
                                ++sizes;
                                ++x;
                            }

                            number = "";
                            break;

                        case '|':
                            float.TryParse(number, out float newPos);

                            if (positions > 0)
                            {
                                zPos = newPos;
                            }
                            else if (positions == 0)
                            {
                                yPos = newPos;
                                ++positions;
                            }
                            else
                            {
                                xPos = newPos;
                                initialXPos = xPos;
                                ++positions;
                            }

                            number = "";
                            break;

                        case '*':
                            uint.TryParse(number, out uint newId);

                            ids.Enqueue(newId);
                            number = "";
                            break;

                        case ')':
                            nodes[x][y][z] = new ScrNode(ids.Dequeue(), new Vector3(xPos, yPos, zPos));
                            RegisterConnections(nodes, ids, x, y, z);
                            ++z;
                            positions = 0;
                            number = "";
                            break;

                        case '\\':
                            ++x;
                            xPos = initialXPos + (x * .5f);
                            y = -2;
                            z = 0;
                            sizes = 0;
                            positions = 0;
                            number = "";
                            break;

                        default:
                            number += item[i];
                            break;
                    }
                }
            }
        }

        private void RegisterConnections(IPathNode[][][] nodes, Queue<uint> ids, int x, int y, int z)
        {
            addedNodes.Add(nodes[x][y][z].ID, nodes[x][y][z]);

            List<NodeConnection> connections = new List<NodeConnection>();

            while (ids.Count > 0)
            {
                uint id = ids.Dequeue();

                if (id > nodes[x][y][z].ID)
                {
                    if (!nodesWaiting.ContainsKey(id))
                        nodesWaiting.Add(id, new());

                    nodesWaiting[id].Enqueue(((x, y, z), 0));
                }
                else
                {
                    //var (nx, ny ,nz) = (addedNodes[id].x, addedNodes[id].y, addedNodes[id].z);
                    connections.Add(new NodeConnection(addedNodes[id], ConnectionType.BIDIMENSIONAL, .5f));
                }
            }

            var nodeCopy = (ScrNode)nodes[x][y][z];
            nodeCopy.SetConnections(connections);
            nodes[x][y][z] = nodeCopy;
        }

        private void SetConnections(IPathNode[][][] nodes)
        {
            foreach (var node in nodesWaiting)
            {
                while (node.Value.Count > 0)
                {
                    ((int x, int y, int z) pos, int idx) id = node.Value.Dequeue();
                    //var nodeCopy = (ScrNode)nodes[id.pos.x][id.pos.y][id.pos.z];
                    //var connections = nodeCopy.NodeConnections;
                    var connections = ((ScrNode)nodes[id.pos.x][id.pos.y][id.pos.z]).NodeConnections;

                    connections ??= new();
                    connections.Add(new NodeConnection(addedNodes[node.Key], ConnectionType.BIDIMENSIONAL, 0.5f));

                    ((ScrNode)nodes[id.pos.x][id.pos.y][id.pos.z]).SetConnections(connections);
                    //nodeCopy.SetConnections(connections);
                    //nodes[id.pos.x][id.pos.y][id.pos.z] = nodeCopy;
                }
            }
        }

        private LinkedList<char[]> LoadFromFile()
        {
            string path = UnityEngine.Application.persistentDataPath + fileName + SceneManager.GetActiveScene().buildIndex + ".txt";
            var text = new LinkedList<char[]>();

            using (Stream stream = new FileStream(path, FileMode.Open))
            {
                int totalRedaded = 0;

                do
                {
                    byte[] bytes = new byte[50];
                    totalRedaded = stream.Read(bytes, 0, bytes.Length);
                    text.AddLast(Encoding.UTF8.GetChars(bytes));
                    
                } while(totalRedaded > 0);
            }

            return text;
        }
    }
}
