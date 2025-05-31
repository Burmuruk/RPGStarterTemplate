using Burmuruk.AI;
using Burmuruk.WorldG.Patrol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Burmuruk.Tesis.Movement.PathFindig
{
    public static class NavSaver
    {
        static bool saved;
        static float radious;
        static float distance;
        static float maxAngle;

        static Dictionary<uint, Queue<((int x, int y, int z) idx, int connectionIdx)>> nodesWaiting;
        static Dictionary<uint, IPathNode> addedNodes;

        const string FILE_NAME = "NavGrid";
        static private bool working = false;
        static bool loaded;
        public static NodeListSuplier NodeList { get; private set; }

        public static event Action OnPathLoaded;

        public static bool Saved { get => saved; }
        public static bool Loaded { get => loaded; private set => loaded = value; }

        public static void Restart()
        {
            if (working) return;

            saved = false;
            nodesWaiting = null;
            addedNodes = null;
            loaded = false;
        }

        public static void SaveList(IPathNode[][][] nodes, int count)
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

        private static void Write(LinkedList<string> text)
        {
            string path = Path.Combine(Application.streamingAssetsPath, FILE_NAME + ".txt");
            //string path = UnityEngine.Application.persistentDataPath + FILE_NAME + SceneManager.GetActiveScene().buildIndex + ".txt";

            using (Stream stream = new FileStream(path, FileMode.Create))
            {
                foreach (var item in text)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(item);
                    stream.Write(bytes, 0, item.Length);
                }
            }
        }

        private static string GetNodeConnections(IPathNode node)
        {
            string connections = "";

            foreach (var cnc in node.NodeConnections)
            {
                connections += cnc.node.ID + "*";
            }

            return connections += ")";
        }

        public static void SaveExtraData(float radious, float distance, float maxAngle)
        {
            NavSaver.distance = distance;
            NavSaver.maxAngle = maxAngle;
            NavSaver.radious = radious;
        }

        private static void SetNodeList(IPathNode[][][] nodes)
        {
            NodeList = new NodeListSuplier(nodes);

            NodeList.SetTarget(radious, distance, maxAngle);
        }

        public static void LoadNavMesh()
        {
            if (Loaded || working) return;

            working = true;
            SynchronizationContext context = SynchronizationContext.Current;
            SetNodeList(GenerateNodesArray());

            Loaded = true;
            working = false;
            //Task<IPathNode[][][]> loadDataTask = Task.Run(() => GenerateNodesArray());
            //loadDataTask.ContinueWith((antecedent) =>
            //{
            //    SetNodeList(antecedent.Result);

            //    Loaded = true;
            //    working = false;

            //    context.Post(_ => OnPathLoaded?.Invoke(), null);
            //}, TaskContinuationOptions.ExecuteSynchronously);
        }

        private static IPathNode[][][] GenerateNodesArray()
        {
            addedNodes = new();
            nodesWaiting = new();

            LinkedList<char[]> text = LoadFromFile();
            GetNodesFromText(text, out IPathNode[][][] nodes);
            SetConnections(nodes);

            return nodes;
        }

        private static void GetNodesFromText(LinkedList<char[]> text, out IPathNode[][][] nodes)
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

        private static void RegisterConnections(IPathNode[][][] nodes, Queue<uint> ids, int x, int y, int z)
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

        private static void SetConnections(IPathNode[][][] nodes)
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

        private static LinkedList<char[]> LoadFromFile()
        {
            string path = Path.Combine(Application.streamingAssetsPath, FILE_NAME + ".txt");
            //string path = UnityEngine.Application.persistentDataPath + FILE_NAME + SceneManager.GetActiveScene().buildIndex + ".txt";
            var text = new LinkedList<char[]>();

            using (Stream stream = new FileStream(path, FileMode.Open))
            {
                int totalRedaded = 0;

                do
                {
                    byte[] bytes = new byte[50];
                    totalRedaded = stream.Read(bytes, 0, bytes.Length);
                    text.AddLast(Encoding.UTF8.GetChars(bytes));

                } while (totalRedaded > 0);
            }

            return text;
        }
    }
}
