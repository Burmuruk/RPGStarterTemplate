using Burmuruk.AI;
using Burmuruk.Tesis.Utilities;
using Burmuruk.WorldG.Patrol;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Burmuruk.Tesis.Movement.PathFindig
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Path", order = 2)]
    public class Path : ScriptableObject
    {
        [SerializeField] bool loaded;
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

        private struct IdData
        {
            public uint Id;
            
            public int idx;
            public int ConnectionIdx;
        }

        public bool Loaded { get => loaded; }

        public void SaveList(IPathNode[][][] nodes, int count)
        {
            InitVariables();
            towers = new ScrNode[count];
            int idx = 0;

            for (int i = 0; i < nodes.Length; i++)
            {
                towerSizes.Add(nodes[i].Length);

                for (int j = 0; j < nodes[i].Length; j++)
                {
                    for (int k = 0; k < nodes[i][j].Length; k++)
                    {
                        ref IPathNode curNode = ref nodes[i][j][k];

                        var node = new ScrNode(curNode.ID, curNode.Position);
                        SetNodeConnections(in curNode, ref node, in idx);

                        towers[idx++] = node;
                    }

                    towerHeights.Add(nodes[i][j].Length);
                }
            }

            SaveConnections();

            loaded = true;
            EditorUtility.SetDirty(this);
            Debug.Log("Saved");
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

        public INodeListSupplier GetNodeList()
        {
            var nodeList = new NodeListSuplier(GetReversedList());

            nodeList.SetTarget(radious, distance, maxAngle);

            return nodeList;
        }

        private IPathNode[][][] GetReversedList()
        {
            var nodes = new IPathNode[towerSizes.Count][][];
            int yidx = 0;
            int idx = 0;

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

        [System.Serializable]
        private struct LinearTower
        {
            [SerializeField] public ScrNode[] nodes;

            public LinearTower(ScrNode[] nodes)
            {
                this.nodes = nodes;
            }
        }
    }
}
