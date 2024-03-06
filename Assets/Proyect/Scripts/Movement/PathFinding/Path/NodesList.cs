using Burmuruk.AI.PathFinding;
using Burmuruk.Collections;
using Burmuruk.WorldG.Patrol;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum pState
{
    None,
    running,
    finished,
    deleting
}

namespace Burmuruk.AI
{
    public class NodesList : MonoBehaviour, INodeListSupplier
    {
        #region Variables
        [Header("Nodes Settings")]
        [Space]
        //[SerializeField]
        //float maxDistance = 3;
        [SerializeField]
        float maxAngle = 45;
        [SerializeField]
        bool showChanges = false;

        [Header("Mesh Settings")]
        [Space]
        [SerializeField]
        GameObject Node;
        [SerializeField]
        float nodDistance = 3;
        [SerializeField]
        GameObject x1;
        [SerializeField]
        GameObject x2;
        [SerializeField]
        bool createMesh = false;
        [SerializeField]
        bool showMeshZone = false;
        [SerializeField]
        float pRadious = .5f;
        [SerializeField]
        bool phisicNodes = false;

        [Header("PathFinding Settings")]
        [Space]
        [SerializeField]
        public GameObject startNode;
        bool nearestStart = false;
        [SerializeField]
        public GameObject endNode;
        bool nearestEnd = false;
        [SerializeField]
        bool drawPath = false;

        Dijkstra dijkstra;
        uint nodeCount = 0;
        List<(IPathNode node, IPathNode hitPos)> edgesToFix;

        public pState dijkstraState = pState.None;
        public pState meshState = pState.None;
        public pState connectionsState = pState.None;

        private LinkedGrid<ScrNode> nodes;
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
                    connectionsState == pState.running)
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
                    dijkstraState == pState.deleting)
                    return true;

                else
                    return false;
            }
        }
        public LinkedGrid<ScrNode> Nodes
        {
            get
            {
                if (connectionsState == pState.finished)
                {
                    if (nodes.Count == 0)
                    {
                        for (int i = 0; i < transform.childCount; i++)
                        {
                            var node = transform.GetChild(i).GetComponent<IPathNode>();

                            if (node != null)
                                nodes.Add((ScrNode)node);
                        }
                    }

                    return nodes;
                }
                else
                    return null;
            }
        }

        public Vector3 StartNode => throw new NotImplementedException();

        public Vector3 EndNode => throw new NotImplementedException();

        ICollection<IPathNode> INodeListSupplier.Nodes => throw new NotImplementedException();
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
            //InitializeNodeLists();
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
            InitializeNodeLists();
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
            var nodes = transform.GetComponentsInChildren<IPathNode>();
            nodeCount = 0;
            this.nodes.Clear();

            foreach (ScrNode node in nodes)
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

        public LinkedGrid<ScrNode> Get_Nodes() => nodes;

        #endregion

        #region Connections
        private void InitializeNodeLists()
        {
            var maxVerticalDis = nodDistance / Mathf.Sin(maxAngle * Mathf.PI / 180);
            edgesToFix = new List<(IPathNode node, IPathNode hitPos)>();

            var enumerator = (LinkedGridEnumerator<LinkedGridNode<ScrNode>, ScrNode>)nodes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                foreach (var key in enumerator.Current.Connections.Keys)
                {
                    var cur = enumerator.Current.Node;
                    var nextHead = enumerator.Current[key];

                    while (nextHead != null)
                    {
                        ScrNode next = null;
                        try
                        {
                            next = nextHead.Node;
                        }
                        catch (NullReferenceException)
                        {

                            throw;
                        }

                        float dis = Get_VerticalDifference(cur, next);

                        if (dis <= maxVerticalDis)
                        {
                            float normal1, normal2;
                            //Vector3 hitPos1, hitPos2;

                            bool hitted1 = Detect_OjbstaclesBetween(cur, next, out normal1);
                            bool hitted2 = Detect_OjbstaclesBetween(next, cur, out normal2);

                            (ConnectionType a, ConnectionType b) types = Get_Types(hitted1, hitted2);

                            if (!hitted1)
                                cur.NodeConnections.Add(
                                    new NodeConnection(cur, next, nodDistance, types.a));
                            if (!hitted2)
                                next.NodeConnections.Add(
                                    new NodeConnection(next, cur, nodDistance, types.b));
                        }

                        nextHead = nextHead[Direction.Down];
                    }
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
        //        for (int i = 0; i < directions.Count; i++)
        //        {
        //            x += directions[i].times * (i + directions[i].increment) * directions[i].x;
        //            y += directions[i].times * (i + directions[i].increment) * directions[i].y;

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

        bool Detect_OjbstaclesBetween(IPathNode nodeA, IPathNode nodeB, out float groundNormal)
        {
            groundNormal = 0;
            RaycastHit[] hit;
            var pointA = nodeA.Position + new Vector3(0, pRadious, 0);
            var pointB = nodeA.Position + new Vector3(0, 2 * pRadious + 1, 0);

            var dir = (nodeB.Position - nodeA.Position);

            hit = Physics.CapsuleCastAll(pointA, pointB, pRadious, dir.normalized, Vector3.Distance(nodeA.Position, nodeB.Position));
            Debug.DrawLine(pointA, pointB);
            Debug.DrawRay(pointA, dir.normalized * Vector3.Distance(nodeA.Position, nodeB.Position));

            bool hitted = false;
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

            int rows = (int)(distances.z / nodDistance) + 1;

            nodes = new LinkedGrid<ScrNode>(rows);

            float xIndex = distances.x / nodDistance;
            float zIndex = distances.z / nodDistance;
            float height = distances.y;

            int zPointsIdx;

            for (float i = 0; i < Mathf.Abs(xIndex); i += nodDistance)
            {
                for (float j = 0; j < Mathf.Abs(zIndex); j += nodDistance)
                {
                    var curPosA = new Vector3()
                    {
                        x = x1.transform.position.x + nodDistance * i,
                        y = x2.transform.position.y,
                        z = x1.transform.position.z - nodDistance * j
                    };

                    //Debug.DrawRay(curPosA, Vector3.down * height, Color.green, 9);
                    Ray hi = new Ray(curPosA, Vector3.down * height);
                    var verticalHits = Detect_Ground(height, hi);

                    if (verticalHits != null)
                    {
                        for (int k = 0; k < verticalHits.Count; k++)
                        {
                            //var start = verticalHits[k] + new Vector3(0, .7f, 0);

                            //if (!Physics.CapsuleCast(start, (start + new Vector3(0, 1, 0)), .5f, new Vector3(0, 1, 0), .1f))
                            //    Create_Node(verticalHits[k]);
                            bool added = false;
                            if (Verify_CapsuleArea(verticalHits[k]))
                            {
                                var newNode = Create_Node(verticalHits[k]);
                                if (added)
                                {
                                    nodes.AddDown(nodes.Last, newNode);
                                }
                                else
                                {
                                    nodes.Add(newNode);
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
                        //hi.origin = hi.origin + Vector3.down * (hits[k].distance + 3);


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

        private ScrNode Create_Node(in Vector3 position)
        {
            var newNode = Instantiate(Node, transform);
            newNode.transform.position = position;
            newNode.transform.name = "Node " + nodeCount.ToString();
            var nodeCs = newNode.GetComponent<ScrNode>();
            nodeCs.SetIndex(nodeCount++);
            
            return nodeCs;
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
            IPathNode[] nodes;
            if (this.nodes.Count <= 0)
                nodes = transform.GetComponentsInChildren<IPathNode>();
            else
                nodes = this.nodes.ToArray();

            float minDistance = float.MaxValue;
            int? index = -1;

            for (int i = 0; i < nodes.Length; i++)
            {
                if (Vector3.Distance(nodes[i].Position, start) is var d && d < minDistance)
                {
                    minDistance = d;
                    index = i;
                }
            }

            return index.HasValue ? nodes[index.Value] : null;
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

        public void SetTarget(IPathNode[] nodes, float pRadious = 0.2F, float maxDistance = 2, float maxAngle = 45, float height = 1)
        {
            //this.nodes = nodes;
            //this.pRadious = pRadious;
            //this.maxDistance = maxDistance;
            //this.maxAngle = maxAngle;
            //this.height = height;

            //CalculateNodesConnections();
            throw new NotImplementedException();
        }

        public void Clear() => nodes = null;

        public void SetNodes(ICollection<IPathNode> nodes)
        {
            this.nodes = (LinkedGrid<ScrNode>)nodes;
        }
        #endregion
    }
}