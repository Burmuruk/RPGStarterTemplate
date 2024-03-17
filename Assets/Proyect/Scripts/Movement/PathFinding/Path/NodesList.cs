using Burmuruk.AI.PathFinding;
using Burmuruk.Collections;
using Burmuruk.Tesis.Movement.PathFindig;
using Burmuruk.WorldG.Patrol;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Burmuruk.AI
{
    public enum pState
    {
        None,
        running,
        finished,
        deleting
    }

    public class NodesList : MonoBehaviour, INodeListSupplier
    {
        #region Variables
        [Header("Nodes Settings")]
        [Space]
        [SerializeField] float maxAngle = 45;
        [SerializeField] bool showChanges = false;

        [Header("Mesh Settings")]
        [Space]
        [SerializeField] GameObject debugNode;
        [SerializeField] float nodDistance = 3;
        [SerializeField] GameObject x1;
        [SerializeField] GameObject x2;
        [SerializeField] bool createMesh = false;
        [SerializeField] bool showMeshZone = false;
        [SerializeField] float pRadious = .5f;
        [SerializeField] bool phisicNodes = false;

        [Header("PathFinding Settings"), Space()]
        [SerializeField] public GameObject startNode;
        [SerializeField] public GameObject endNode;
        [SerializeField] bool drawPath = false;

        [Header("Saving Settings"), Space()]
        [SerializeField] public ScriptableObject pathWriter;
        INodeListSaver writer;
        IPathNode[][][] connections;

        [Header("Status"), Space()]
        [SerializeField, Space()] uint nodeCount = 0;
        public pState dijkstraState = pState.None;
        public pState meshState = pState.None;
        public pState connectionsState = pState.None;
        public pState memoryFreed = pState.None;

        Dijkstra dijkstra;
        bool nearestStart = false;
        bool nearestEnd = false;

        List<(IPathNode node, IPathNode hitPos)> edgesToFix;
        private LinkedGrid<IPathNode> nodes;
        #endregion

        #region Properties
        public bool CreateMesh { get => createMesh; set => createMesh = value; }
        public bool HasDijkstra
        {
            get
            {
                if (dijkstra != null && dijkstra.Calculated)
                    return true;
                else
                    return false;
            }
        }
        public pState DijkstraState { get => dijkstraState; }
        public pState MeshState { get => meshState; set => meshState = value; }
        public pState ConnectionsState { get => connectionsState; }
        public bool AreProcessRunning
        {
            get
            {
                if (dijkstraState == pState.running ||
                    meshState == pState.running ||
                    connectionsState == pState.running ||
                    memoryFreed == pState.running)
                    return true;
                else
                    return false;
            }
        }
        public bool AreProcessDeleting
        {
            get
            {
                if (meshState == pState.deleting ||
                    connectionsState == pState.deleting ||
                    dijkstraState == pState.deleting ||
                    memoryFreed == pState.running)
                    return true;

                else
                    return false;
            }
        }

        public Vector3 StartNode => throw new NotImplementedException();

        public Vector3 EndNode => throw new NotImplementedException();

        public IEnumerable<IPathNode> Nodes
        {
            get
            {
                return nodes;
            }
        }
        #endregion

        #region Unity methods
        private void Start()
        {
            if (!x1 || !x2)
            {
                Debug.LogError("All start nodes are not settled.");
                return;
            }

            //if (createMesh)
            //    Create_PathMesh();
            //CalculateConnections();
        }

        private void OnDrawGizmos()
        {
            if (showChanges)
                Draw_Mesh();

            if (showMeshZone)
                Draw_MeshZone();

            if (drawPath && dijkstra != null && dijkstra.Calculated)
                Draw_Dijkstra();
        }
        #endregion

        #region Public methods
        public void Calculate_PathMesh()
        {
            if (!createMesh || AreProcessRunning || AreProcessDeleting) return;

            Destroy_Nodes();
            meshState = pState.running;
            Create_PathMesh();
        }

        public void CalculateNodesConnections()
        {
            if (AreProcessRunning || AreProcessDeleting) return;

            ClearNodeConnections();
            connectionsState = pState.running;
            CalculateConnections();
        }

        public void Calculate_Dijkstra()
        {
            if (!startNode || !endNode) return;
            if (AreProcessRunning || AreProcessDeleting) return;

            nearestStart = startNode.GetComponent<IPathNode>() == null ? true : false;
            nearestEnd = endNode.GetComponent<IPathNode>() == null ? true : false;



            (IPathNode start, IPathNode end) = (nearestStart, nearestEnd) switch
            {
                (true, true) => (FindNearestNode(startNode.transform.position), FindNearestNode(endNode.transform.position)),
                (true, false) => (FindNearestNode(startNode.transform.position), endNode.GetComponent<IPathNode>()),
                (false, true) => (startNode.GetComponent<IPathNode>(), FindNearestNode(endNode.transform.position)),
                _ => (startNode.GetComponent<IPathNode>(), endNode.GetComponent<IPathNode>())
            };

            if (dijkstra != null)
                Clear_Dijkstra();

            dijkstra = new Dijkstra(start, end);
            dijkstraState = pState.running;
            dijkstra.Start_Algorithm(out _);

            Debug.DrawLine(start.Position, start.Position + Vector3.up * 25, Color.red, 10);
            Debug.DrawLine(end.Position, end.Position + Vector3.up * 25, Color.red, 10);

            if (dijkstra.Calculated)
                dijkstraState = pState.finished;
            else
                dijkstraState = pState.None;
        }

        public void Destroy_Nodes()
        {
            if (AreProcessRunning || AreProcessDeleting || meshState != pState.finished) return;

            meshState = pState.deleting;
            var nodes = transform.GetComponentsInChildren<DebugNode>();
            nodeCount = 0;
            this.nodes.Clear();

            foreach (DebugNode node in nodes)
            {
#if UNITY_EDITOR
                DestroyImmediate(node.gameObject);
                continue;
#endif

                Destroy(node.gameObject);
            }

            CreateMesh = true;
            meshState = pState.None;
        }

        public void ClearNodeConnections()
        {
            if (AreProcessRunning || AreProcessDeleting || connectionsState != pState.finished) return;

            connectionsState = pState.deleting;
            var nodes = transform.GetComponentsInChildren<IPathNode>();

            foreach (var node in nodes)
                node.ClearConnections();

            if (HasDijkstra)
                Clear_Dijkstra();

            connectionsState = pState.None;
        }

        public void Clear_Dijkstra()
        {
            if (AreProcessDeleting || AreProcessRunning) return;

            dijkstraState = pState.deleting;
            var nodes = transform.GetComponentsInChildren<IPathNode>();

            if (dijkstra != null)
                dijkstra.Clear();

            dijkstraState = pState.None;
        }

        public IPathFinder Get_PathTo(Vector3 destiny)
        {
            return null;
        }

        public LinkedGrid<IPathNode> Get_Nodes() => nodes;

        #endregion

        #region Connections
        private void CalculateConnections()
        {
            var maxVerticalDis = nodDistance / Mathf.Sin(maxAngle * Mathf.PI / 180);
            edgesToFix = new List<(IPathNode node, IPathNode hitPos)>();

            var enumerator = (LinkedGridEnumerator<IPathNode>)nodes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var curLinkedNode = enumerator.CurrentLinkedNode;
                
                while (curLinkedNode != null)
                {
                    var cur = curLinkedNode.Node;

                    foreach (var direction in curLinkedNode.Connections.Keys)
                    {
                        LinkedGridNode<IPathNode> nextLinkedNode = curLinkedNode[direction];

                        switch (direction)
                        {
                            case Direction.Previous:
                            case Direction.Down:
                            case Direction.Left:
                                continue;

                            case Direction.Next:
                                if (nextLinkedNode.ColumnIdx > curLinkedNode.ColumnIdx)
                                    continue;

                                break;

                            default:
                                break;
                        }

                        while (nextLinkedNode != null)
                        {
                            IPathNode next = nextLinkedNode.Node;
                            float dis = Get_VerticalDifference(cur, next);

                            if (dis <= maxVerticalDis)
                            {
                                float normal1, normal2;

                                bool hitted1 = Detect_OjbstaclesBetween(cur, next, out normal1);
                                bool hitted2 = Detect_OjbstaclesBetween(next, cur, out normal2);

                                CreateConnectionsBetween(cur, next, hitted1, hitted2);
                            }

                            nextLinkedNode = nextLinkedNode[Direction.Up];
                        }
                    }
                    
                    curLinkedNode = curLinkedNode[Direction.Up];
                }
            }

            connectionsState = pState.finished;

            (ConnectionType a, ConnectionType b) Get_Types(bool hitted1, bool hitted2)
            {
                return (hitted1, hitted2) switch
                {
                    (false, false) => (ConnectionType.BIDIMENSIONAL, ConnectionType.BIDIMENSIONAL),
                    (false, true) => (ConnectionType.A_TO_B, ConnectionType.None),
                    (true, false) => (ConnectionType.None, ConnectionType.B_TO_A),
                    _ => (ConnectionType.None, ConnectionType.None),
                };
            }

            float Get_VerticalDifference(IPathNode node, IPathNode cur)
            {
                float dif = 0;

                if (node.Position.y > cur.Position.y)
                {
                    dif = node.Position.y - cur.Position.y;
                }
                else
                    dif = cur.Position.y - node.Position.y;

                return dif;
            }

            void CreateConnectionsBetween(IPathNode cur, IPathNode next, bool hitted1, bool hitted2)
            {
                (ConnectionType a, ConnectionType b) types = Get_Types(hitted1, hitted2);

                if (!hitted1)
                {
                    IPathNode curRef = cur;
                    IPathNode nextRef = next;
                    cur.NodeConnections.Add(
                        new NodeConnection(ref curRef, ref nextRef, nodDistance, types.a));
                }
                if (!hitted2)
                {
                    IPathNode curRef = cur;
                    IPathNode nextRef = next;
                    next.NodeConnections.Add(
                        new NodeConnection(ref nextRef, ref curRef, nodDistance, types.b));
                }
            }
        }

        //private (int x, int y)[] GetNearCoordinates(float maxDistance)
        //{
        //    List<(int x, int y, int times, int increment)> directions = new()
        //    {
        //        { (1, 0, 1, 0) },
        //        { (0, -1, 1, 1) },
        //        { (-1, 0, 2, 2) },
        //        { (0, 1, 2, 1) },
        //        { (1, 0, 2, 1) },
        //        { (0, -1, 1, 1) },
        //    };

        //    List<(int x, int y)> verticalHits = new();
        //    int x = 0;
        //    int y = 0;

        //    for (int j = 0; j < (int)maxDistance; j++)
        //    {
        //        for (int j = 0; j < directions.Count; j++)
        //        {
        //            x += directions[j].times * (j + directions[j].increment) * directions[j].x;
        //            y += directions[j].times * (j + directions[j].increment) * directions[j].y;

        //            if (MathF.Pow(x, 2) + MathF.Pow(y, 2) <= MathF.Pow(maxDistance, 2))
        //            {
        //                verticalHits.Add((x, y));
        //            }
        //        } 
        //    }

        //    return verticalHits.ToArray();
        //}

        //private void Set_OffsetOnEdge(IPathNode a, IPathNode hitPos)
        //{
        //    float distBetween = Vector3.Distance(a.Position, hitPos.Position);
        //    //float disToHit;
        //    var direction = hitPos.Position - a.Position;

        //    RaycastHit hit;
        //    if (!Physics.Raycast(a.Position, direction, out hit, distBetween) &&
        //        !Physics.Raycast(hitPos.Position, direction * -1, out hit, distBetween))
        //        return;

        //    if (MinDistance < pRadious / 2)
        //    {

        //    }
        //    else if (distBetween < pRadious)
        //    {

        //    }
        //    else
        //    {
        //        //var finalPos = direction + direction.normalized * (disToHit - pRadious);
        //        //transform.position = direction + direction.normalized * (disToHit - pRadious);
        //        Debug.DrawLine(a.Position, hit.point + Vector3.up * 10, Color.red, 5);
        //    }
        //}

        private float Get_Magnitud(IPathNode nodeA, IPathNode nodeB) =>
            Vector3.Distance(nodeA.Position, nodeB.Position);

        bool Detect_OjbstaclesBetween(in IPathNode nodeA, in IPathNode nodeB, out float groundNormal)
        {
            groundNormal = 0;
            RaycastHit[] hit;
            var pointA = nodeA.Position + new Vector3(0, pRadious, 0);
            var pointB = nodeA.Position + new Vector3(0, 2 * pRadious + 1, 0);

            var dir = (nodeB.Position - nodeA.Position);

            bool hitted = false;
            hit = Physics.CapsuleCastAll(pointA, pointB, pRadious, dir.normalized, Vector3.Distance(nodeA.Position, nodeB.Position));
            //Debug.DrawLine(pointA, pointB);
            //Debug.DrawRay(pointA, dir.normalized * Vector3.Distance(nodeA.Position, nodeB.Position));

            for (int k = 0; k < hit.Length; k++)
            {
                if (Vector3.Angle(new Vector3(0, 1, 0), hit[k].normal) is var a && (a < (10) || (a > 89 && a < 90.5)) && a != 0)
                {
                    hitted = true;
                }
                else if (Vector3.Angle(new Vector3(0, 0, hit[k].normal.z), new Vector3(0, 0, 1)) is var a2 && (a2 < maxAngle || a2 == 0))
                {
                    groundNormal = a2;
                }
            }

            return hitted;
        }
        #endregion

        #region Mesh
        private void Create_PathMesh()
        {
            Vector3 distances = Fix_InvertedPositions();

            int rows = (int)(distances.z / nodDistance);

            nodes = new LinkedGrid<IPathNode>(rows);

            int xIndex = (int)(distances.x / nodDistance);
            int zIndex = (int)(distances.z / nodDistance);
            int height = (int)(distances.y);
            int columnIdx = 0;

            for (float i = 0; i < Mathf.Abs(xIndex); i += nodDistance, columnIdx++)
            {
                int idx = 0;
                for (float j = 0; j < Mathf.Abs(zIndex); j += nodDistance, idx++)
                {
                    var curPosA = new Vector3()
                    {
                        x = x1.transform.position.x + nodDistance * i,
                        y = x2.transform.position.y,
                        z = x1.transform.position.z - nodDistance * j
                    };

                    Ray hi = new Ray(curPosA, Vector3.down * height);

                    var verticalHits = Detect_Ground(height, hi);
                    
                    if (verticalHits != null)
                    {
                        bool added = false;
                        for (int k = 0; k < verticalHits.Count; k++)
                        {
                            if (Verify_CapsuleArea(verticalHits[k]))
                            {
                                ScrNode newNode; 
                                Create_Node(verticalHits[k], out newNode);

                                if (added == true)
                                {
                                    nodes.AddUp(nodes.Last, newNode);
                                }
                                else
                                {
                                    nodes.Add(newNode, idx, columnIdx);
                                    added = true;
                                }
                            }
                        }
                    }
                }
            }

            CreateMesh = false;
            meshState = pState.finished;
        }

        private Vector3 Fix_InvertedPositions()
        {
            return new Vector3()
            {
                x = GetSize(x1.transform.position.x, x2.transform.position.x),
                y = GetSize(x1.transform.position.y, x2.transform.position.y),
                z = GetSize(x1.transform.position.z, x2.transform.position.z),
            };

            
            //if (x2.transform.position.z < x1.transform.position.z)
            //{
            //    var newPos = x1.transform.position;
            //    x1.transform.position = x2.transform.position;
            //    x2.transform.position = newPos;
            //}
            float GetSize(float x1, float x2)
            {
                return (x1 < 0, x2 < 0) switch
                {
                    (true, true) => MathF.Abs(x1 + x2),
                    (false, true) => x1 + MathF.Abs(x2),
                    (true, false) => MathF.Abs(x1) + x2,
                    (false, false) => x1 + x2
                };
            }
        }

        private List<Vector3> Detect_Ground(float height, Ray hi, Vector3 offset = default)
        {
            List<Vector3> nodes = null;
            var offsetRay = hi;
            offsetRay.origin = hi.origin + offset;

            var hits = Physics.RaycastAll(offsetRay, height);

            if (hits != null && hits.Length > 0)
            {
                for (int k = 0; k < hits.Length; k++)
                {
                    var angle = Vector3.Angle(hits[k].normal, Vector3.up);

                    if (angle <= maxAngle)
                    {
                        (nodes ??= new List<Vector3>()).Add(offsetRay.origin + Vector3.down * (hits[k].distance - .1f)/* + Vector3.up * 1.5f*/);

                        offset += Vector3.down * (hits[k].distance + 3);

                        var positions = Detect_Ground(height - hits[k].distance - 3, hi, offset);

                        if (positions != null)
                            foreach (var pos in positions)
                            {
                                nodes.Add(pos);
                            }
                    }
                }
            }

            return nodes;
        }

        private bool Verify_CapsuleArea(Vector3 position)
        {
            var start = position + new Vector3(0, .7f, 0);

            if (!Physics.CapsuleCast(start, (start + new Vector3(0, 1, 0)), .5f, new Vector3(0, 1, 0), .1f))
                return true;

            return false;
        }

        private void Create_Node(in Vector3 position, out ScrNode node)
        {
            node = new ScrNode(nodeCount++, position);

            if (!phisicNodes) return;
            
            var newNode = Instantiate(debugNode, transform);
            newNode.transform.position = position;
            newNode.transform.name = "Node " + node.ID.ToString();
            var nodeCs = newNode.GetComponent<DebugNode>();
            nodeCs.SetNode(node);
        }

        private void Draw_MeshZone()
        {
            Vector3 dis = x2.transform.position - x1.transform.position;
            Debug.DrawLine(x1.transform.position, x1.transform.position + Vector3.right * dis.x, Color.red, 2);
            Debug.DrawLine(x1.transform.position, x1.transform.position + Vector3.forward * dis.z, Color.red, 2);
            Debug.DrawLine(x1.transform.position, x1.transform.position + Vector3.up * dis.y, Color.red, 2);

            Debug.DrawLine(x2.transform.position, x2.transform.position + Vector3.left * dis.x, Color.red, 2);
            Debug.DrawLine(x2.transform.position, x2.transform.position + Vector3.back * dis.z, Color.red, 2);
            Debug.DrawLine(x2.transform.position, x2.transform.position + Vector3.down * dis.y, Color.red, 2);

            var rd = x2.transform.position + Vector3.down * dis.y + Vector3.left * dis.x;
            Debug.DrawLine(rd, rd + Vector3.up * dis.y, Color.red, 2);
            Debug.DrawLine(rd, rd + Vector3.right * dis.x, Color.red, 2);
            Debug.DrawLine(rd + Vector3.up * dis.y, rd + Vector3.up * dis.y + Vector3.back * dis.z, Color.red, 2);

            var ld = x1.transform.position + Vector3.up * dis.y + Vector3.right * dis.x;
            Debug.DrawLine(ld, ld + Vector3.down * dis.y, Color.red, 2);
            Debug.DrawLine(ld, ld + Vector3.left * dis.x, Color.red, 2);
            Debug.DrawLine(ld + Vector3.down * dis.y, ld + Vector3.down * dis.y + Vector3.forward * dis.z, Color.red, 2);
        }

        private void Draw_Mesh()
        {
            var nodes = transform.GetComponentsInChildren<IPathNode>();

            for (int i = 0; i < nodes.Length; i++)
            {
                var cur = nodes[i];

                for (int j = i + 1; j < nodes.Length; j++)
                {
                    if (Get_Magnitud(cur, nodes[j]) is var m && m <= nodDistance)
                    {
                        bool hitted1 = Detect_OjbstaclesBetween(cur, nodes[j], out _);
                        bool hitted2 = Detect_OjbstaclesBetween(nodes[j], cur, out _);

                        if (!hitted1 && !hitted2)
                            Debug.DrawLine(cur.Position + new Vector3(0, 1.5f, 0), (nodes[j].Position) + new Vector3(0, 1.5f, 0), Color.red);
                    }
                }
            }
        }
        #endregion

        #region Dijkstra
        public IPathNode FindNearestNode(Vector3 start)
        {
            if (connections == null || connections.Length <= 0) return null;

            (int x, int y, int z)? index = null;
            int length = connections.Length - 1;

            for (int i = 0; i < connections.Length; i++)
            {
                if (i == length ||
                    (start.x >= connections[i][0][0].Position.x && start.x < connections[i + 1][0][0].Position.x))
                {
                    length = connections[i].Length - 1;

                    for (int j = 0; j < connections[i].Length; j++)
                    {
                        if (j == length ||
                            (start.z >= connections[i][j][0].Position.z))
                        {
                            length = connections[i][j].Length - 1;

                            for (int k = 0; k < connections[i][j].Length; k++)
                            {
                                if (k == length || 
                                    (start.y >= connections[i][j][k].Position.y && start.y < connections[i][j][k + 1].Position.y))
                                {
                                    index = (i, j, k);
                                    break;
                                }
                            }

                            break;
                        }
                    }

                    break;
                }
            }

            return index.HasValue ? connections[index.Value.x][index.Value.y][index.Value.z] : null;
        }

        private void Draw_Dijkstra()
        {
            var path = dijkstra.ShortestPath;
            var node = path.First;

            for (int i = 0; i < path.Count - 1; i++)
            {
                Debug.DrawLine(node.Value.Position + Vector3.up, node.Next.Value.Position + Vector3.up, Color.black);

                node = node.Next;
            }
        }

        #endregion

        #region List supplier
        public void SetTarget(IPathNode[] nodes, float pRadious = 0.2F, float maxDistance = 2, float maxAngle = 45, float height = 1)
        {
            this.connections = null;
            this.pRadious = pRadious;
            this.nodDistance = maxDistance;
            this.maxAngle = maxAngle;
            //this.height = height;

            //CalculateNodesConnections();
        }

        public void Clear() => nodes = null;

        public void SetNodes(ICollection<IPathNode> nodes)
        {
            this.nodes = (LinkedGrid<IPathNode>)nodes;
        }

        public void SetConnections(IPathNode[][][] nodes)
        {
            connections = nodes;
        }

        public IPathNode[][][] FreeMemory()
        {
            if (nodes == null || nodes.Count <= 0) return null;

            memoryFreed = pState.running;
            IPathNode[][][] connections = null;

            try
            {
                connections = nodes.ToArray();
            }
            catch (OverflowException)
            {
                Debug.LogError("The amount of nodes is too big to proceed");
                return null;
            }

            memoryFreed = pState.None;
            //Destroy_Nodes();

            memoryFreed = pState.deleting;
            meshState = pState.None;
            connectionsState = pState.None;
            nodeCount = 0;

            memoryFreed = pState.finished;

            return connections;
        }

        public void SaveList()
        {
            if (memoryFreed != pState.None || pathWriter == null) return;

            if (pathWriter is INodeListSaver saver && saver != null)
            {
                connections = null;
                connections = FreeMemory();

                var nodeList = new NodeListSuplier();
                //SerializedObject serializedObj = new UnityEditor.SerializedObject(pathWriter);
                //SerializedProperty myList = serializedObj.FindProperty("m_nodeList");

                nodeList.SetConnections(connections);

                //myList.managedReferenceValue = nodeList;
                saver.SaveList(this);
            }
        }

        public void LoadList()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}