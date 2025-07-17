using Burmuruk.AI.PathFinding;
using Burmuruk.Collections;
using Burmuruk.RPGStarterTemplate.Movement.PathFindig;
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

    public class NodesList : ScriptableObject
    {
        #region Variables
        [Header("Nodes Settings")]
        [Space]
        [SerializeField] float nodeDistance = 3;
        [SerializeField] float playerRadious = .5f;
        [SerializeField] float maxAngle = 45;
        [HideInInspector] bool showChanges = false; //hidded due to optimization issues

        [Header("Mesh Settings")]
        [Space]
        [SerializeField] public GameObject debugNode;
        [HideInInspector] public GameObject x1;
        [HideInInspector] public GameObject x2;
        [SerializeField] public bool showMeshZone = false;
        [SerializeField] bool debugNodes = false;
        bool canCreateMesh = false;
        bool addDynamicObjs = true;
        int layer;

        [Header("Saving Settings"), Space()]
        IPathNode[][][] connections;

        uint nodeCount = 0;
        pState meshState = pState.None;
        pState connectionsState = pState.None;
        pState memoryFreed = pState.None;

        List<(IPathNode node, IPathNode hitPos)> edgesToFix;
        private LinkedGrid<IPathNode> nodes;
        private Func<bool, Vector3> _areaDetector;
        #endregion

        #region Properties
        public bool CanCreateMesh { get => canCreateMesh; set => canCreateMesh = value; }
        public pState MeshState { get => meshState; set => meshState = value; }
        public pState ConnectionsState { get => connectionsState; }
        public bool AreProcessRunning
        {
            get
            {
                if (meshState == pState.running ||
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
                    memoryFreed == pState.running)
                    return true;

                else
                    return false;
            }
        }
        public IEnumerable<IPathNode> Nodes
        {
            get
            {
                return nodes;
            }
        }
        public float NodeDistance => nodeDistance;
        public float PlayerRadious => playerRadious;
        public float MaxAngle => maxAngle;
        #endregion

        #region Unity methods
        private void Start()
        {
            if (!x1 || !x2)
            {
                Debug.LogError("All start nodes are not settled.");
                return;
            }
        }

        private void OnDrawGizmos()
        {
            if (showChanges)
                Draw_Mesh();

            if (showMeshZone)
                Draw_MeshZone();
        }
        #endregion

        #region Public methods
        public void SetAvailableAreaDetector(Func<bool, Vector3> detector) => 
            _areaDetector = detector;

        public void Calculate_PathMesh()
        {
            if (!canCreateMesh || AreProcessRunning || AreProcessDeleting) return;

            Destroy_Nodes();
            meshState = pState.running;
            Create_PathMesh();

            if (addDynamicObjs)
                DisableDynamicObjects();
        }

        public void CalculateNodesConnections()
        {
            if (AreProcessRunning || AreProcessDeleting) return;

            ClearNodeConnections();
            connectionsState = pState.running;
            CalculateConnections();
        }

        public void Destroy_Nodes()
        {
            if (AreProcessRunning || AreProcessDeleting || meshState != pState.finished) return;

            meshState = pState.deleting;
//            var nodes = transform.GetComponentsInChildren<DebugNode>();
//            nodeCount = 0;
//            this.nodes.Clear();

//            foreach (DebugNode node in nodes)
//            {
//#if UNITY_EDITOR
//                DestroyImmediate(node.gameObject);
//                continue;
//#endif

//                Destroy(node.gameObject);
//            }

            CanCreateMesh = true;
            meshState = pState.None;
        }

        public void ClearNodeConnections()
        {
            if (AreProcessRunning || AreProcessDeleting || connectionsState != pState.finished) return;

            //connectionsState = pState.deleting;
            //var nodes = transform.GetComponentsInChildren<IPathNode>();

            //foreach (var node in nodes)
            //    node.ClearConnections();

            connectionsState = pState.None;
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
            var maxVerticalDis = nodeDistance * Mathf.Sin(maxAngle * Mathf.PI / 180);
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
                        CalculateConnectionTo(in maxVerticalDis, curLinkedNode, cur, in direction);
                    }

                    if (curLinkedNode.Node.NodeConnections == null || curLinkedNode.Node.NodeConnections.Count <= 0)
                    {
                        nodes.Remove(curLinkedNode);

                        if (curLinkedNode[Direction.Previous] != null)
                        {
                            var prevLinkedNode = curLinkedNode[Direction.Previous];
                            CalculateConnectionTo(in maxVerticalDis, prevLinkedNode, prevLinkedNode.Node, Direction.Next);
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
                        new NodeConnection(ref curRef, ref nextRef, nodeDistance, types.a));
                }
                if (!hitted2)
                {
                    IPathNode curRef = cur;
                    IPathNode nextRef = next;
                    next.NodeConnections.Add(
                        new NodeConnection(ref nextRef, ref curRef, nodeDistance, types.b));
                }
            }

            void CalculateConnectionTo(in float maxVerticalDis, LinkedGridNode<IPathNode> curLinkedNode, IPathNode cur, in Direction direction)
            {
                LinkedGridNode<IPathNode> nextLinkedNode = null;

                switch (direction)
                {
                    case Direction.Previous:
                    case Direction.Down:
                    case Direction.Left:
                        return;

                    case Direction.Next:
                        nextLinkedNode = curLinkedNode[direction];

                        if (nextLinkedNode == null) 
                            return;
                        if (nextLinkedNode.ColumnIdx > curLinkedNode.ColumnIdx)
                            return;
                        if (nextLinkedNode.RowIdx - curLinkedNode.RowIdx > 1) 
                            return;
                        break;

                    case Direction.Right:
                        nextLinkedNode = curLinkedNode[direction];

                        if (nextLinkedNode == null)
                            return;
                        if (nextLinkedNode.ColumnIdx - curLinkedNode.ColumnIdx > 1)
                            return;
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

                        if (!hitted1 && !hitted2)
                            CreateConnectionsBetween(cur, next, hitted1, hitted2);
                    }

                    nextLinkedNode = nextLinkedNode[Direction.Up];
                }
            }
        }

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
        //        Debug.DrawLine(a.Position, hit.position + Vector3.up * 10, Color.red, 5);
        //    }
        //}

        private float Get_Magnitud(IPathNode nodeA, IPathNode nodeB) =>
            Vector3.Distance(nodeA.Position, nodeB.Position);

        bool Detect_OjbstaclesBetween(in IPathNode nodeA, in IPathNode nodeB, out float groundNormal)
        {
            groundNormal = 0;
            RaycastHit[] hit;
            var pointA = nodeA.Position + new Vector3(0, playerRadious + .01f, 0);
            var pointB = nodeA.Position + new Vector3(0, 2 * playerRadious + 1 + .01f, 0);

            var dir = (nodeB.Position - nodeA.Position);

            bool hitted = false;
            hit = Physics.CapsuleCastAll(pointA, pointB, playerRadious, dir.normalized, Vector3.Distance(nodeA.Position, nodeB.Position), 1 << layer);
            //Debug.DrawLine(pointA, pointB);
            //Debug.DrawRay(pointA, nextDir.normalized * Vector3.Distance(nodeA.Position, nodeB.Position));

            for (int k = 0; k < hit.Length; k++)
            {
                if (Vector3.Angle(new Vector3(0, 1, 0), hit[k].normal) is var a && (a < (10) || (a > 89 && a < 90.5)) && a != 0)
                {
                    hitted = true;
                    //Debug.DrawRay(hit[k].point, hit[k].normal, Color.yellow, 8);
                    //Debug.DrawLine(pointA, pointB, Color.yellow, 8);
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

            int rows = (int)(distances.z / nodeDistance);

            nodes = new LinkedGrid<IPathNode>(rows);

            int xIndex = (int)(distances.x / nodeDistance);
            int zIndex = (int)(distances.z / nodeDistance);
            int height = (int)(distances.y);
            int columnIdx = 0;

            for (int i = 0; i < Mathf.Abs(xIndex); i++, columnIdx++)
            {
                int idx = 0;
                for (int j = 0; j < Mathf.Abs(zIndex); j++, idx++)
                {
                    var curPosA = new Vector3()
                    {
                        x = x1.transform.position.x + nodeDistance * i,
                        y = x2.transform.position.y,
                        z = x1.transform.position.z - nodeDistance * j
                    };

                    Ray detectionRay = new Ray(curPosA, Vector3.down);

                    var verticalHits = Detect_Ground(height, detectionRay, out List<Collider> hittedCols);

                    if (verticalHits != null)
                    {
                        bool added = false;
                        verticalHits = verticalHits.OrderBy(hit => hit.magnitude).ToList();

                        for (int k = 0; k < verticalHits.Count; k++)
                        {
                            if (Verify_CapsuleArea(verticalHits[k]))
                            {
                                NodeData newNode;
                                Create_Node(verticalHits[k], out newNode);

                                if (hittedCols[k].gameObject.TryGetComponent(out DynamicNavObject dObj))
                                    dObj.Nodes.Add(newNode.ID);

                                if (added == true)
                                {
                                    if (newNode.Position.y > nodes.Last.Node.Position.y)
                                        nodes.AddUp(nodes.Last, newNode);
                                    else
                                        nodes.AddDown(nodes.Last, newNode);
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

            CanCreateMesh = false;
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
                    (true, true) => MathF.Abs(x1 - x2),
                    (false, true) => x1 + MathF.Abs(x2),
                    (true, false) => MathF.Abs(x1) + x2,
                    (false, false) => MathF.Abs(x1 - x2),
                };
            }
        }

        private List<Vector3> Detect_Ground(float distance, Ray detectionRay, out List<Collider> collidersHitted)
        {
            collidersHitted = null;
            List<Vector3> nodes = new();
            LinkedList<Collider> colliders = new();
            var offsetRay = detectionRay;
            RaycastHit hit;

            while (distance > 0)
            {
                if (Physics.Raycast(offsetRay, out hit, distance, 1 << layer))
                {
                    var angle = Vector3.Angle(hit.normal, Vector3.up);

                    if (angle <= maxAngle && hit.distance > .1f)
                    {
                        foreach (var collider in colliders)
                        {
                            if (collider.bounds.Contains(hit.point))
                                goto EndDetection;
                        }

                        nodes.Add(hit.point);
                        (collidersHitted ??= new()).Add(hit.collider);
                    }
                }
                else break;

                EndDetection:
                if (!colliders.Contains(hit.collider)) 
                    colliders.AddLast(hit.collider);

                offsetRay.origin += Vector3.down * (hit.distance + 1)/* + Vector3.up * 1.5f*/;
                distance -= hit.distance + 1;
            }

            return nodes;
        }

        private bool Verify_CapsuleArea(Vector3 position)
        {
            var start = position + new Vector3(0, .7f, 0);

            if (!Physics.CapsuleCast(start, (start + new Vector3(0, 1, 0)), .5f, new Vector3(0, 1, 0), .1f, 1 << layer))
                return true;

            return false;
        }

        private void Create_Node(in Vector3 position, out NodeData node)
        {
            node = new NodeData(nodeCount++, position);

            if (!debugNodes) return;

            //var newNode = Instantiate(debugNode, transform);
            //newNode.transform.position = position;
            //newNode.transform.name = "Node " + node.ID.ToString();
            //var nodeCs = newNode.GetComponent<DebugNode>();
            //nodeCs.SetNode(in node);
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
            //var nodes = transform.GetComponentsInChildren<IPathNode>();

            //for (int i = 0; i < nodes.Length; i++)
            //{
            //    var cur = nodes[i];

            //    for (int j = i + 1; j < nodes.Length; j++)
            //    {
            //        if (Get_Magnitud(cur, nodes[j]) is var m && m <= nodDistance)
            //        {
            //            bool hitted1 = Detect_OjbstaclesBetween(cur, nodes[j], out _);
            //            bool hitted2 = Detect_OjbstaclesBetween(nodes[j], cur, out _);

            //            if (!hitted1 && !hitted2)
            //                Debug.DrawLine(cur.Position + new Vector3(0, 1.5f, 0), (nodes[j].Position) + new Vector3(0, 1.5f, 0), Color.red);
            //        }
            //    }
            //}
        }

        private void DisableDynamicObjects()
        {
            //DynamicNavObject[] dynamicNavObjects = FindObjectsOfType<DynamicNavObject>();

            //foreach (var dynamicNavObject in dynamicNavObjects)
            //{
            //    if (!dynamicNavObject.IsEnable) continue;

            //    foreach (var node in dynamicNavObject.Nodes)
            //    {
            //        //node.Enable(false);
            //    }
            //}
        }
        #endregion


        #region List supplier

        public void Clear() => nodes = null;

        public void SetNodes(ICollection<IPathNode> nodes)
        {
            this.nodes = (LinkedGrid<IPathNode>)nodes;
        }

        public (IPathNode[][][], int length) FreeMemory()
        {
            if (nodes == null || nodes.Count <= 0) return default;

            memoryFreed = pState.running;
            IPathNode[][][] connections = null;
            int length = 0;

            try
            {
                (connections, length) = nodes.ToArray();
            }
            catch (OverflowException)
            {
                Debug.LogError("The amount of nodes is too big to proceed");
                return default;
            }

            memoryFreed = pState.None;
            //Destroy_Nodes();

            memoryFreed = pState.deleting;
            meshState = pState.None;
            connectionsState = pState.None;
            nodeCount = 0;

            memoryFreed = pState.finished;

            return (connections, length);
        }

        public void SaveList()
        {
            if (memoryFreed != pState.None) return;

            connections = null;
            int length = 0;
            (connections, length) = FreeMemory();

            //var nodeList = new NodeListSuplier(connections);
            //SerializedObject serializedObj = new UnityEditor.SerializedObject(Path);
            //SerializedProperty myList = serializedObj.FindProperty("m_nodeList");

            //nodeList.SetTarget(pRadious, nodDistance, MaxAngle);

            //myList.managedReferenceValue = nodeList;
            NavSaver.SaveExtraData(playerRadious, nodeDistance, MaxAngle);
            NavSaver.SaveList(connections, length);
        }

        public void LoadList()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}