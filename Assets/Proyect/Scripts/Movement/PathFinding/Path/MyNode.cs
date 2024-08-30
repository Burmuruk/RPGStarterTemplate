using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.WorldG.Patrol
{
    public enum ConnectionType
    {
        None,
        BIDIMENSIONAL,
        A_TO_B,
        B_TO_A
    }
    [System.Serializable]
    public struct NodeConnection
    {
        public IPathNode node;
        public ConnectionType connectionType;
        public float Magnitude { get; private set; }

        public NodeConnection(ref IPathNode current, ref IPathNode node)
        {
            this.node = node;
            this.connectionType = ConnectionType.None;
            this.Magnitude = 0;
            
            Magnitude = DistanceBewtweenNodes(current, node);
        }

        public NodeConnection(ref IPathNode current, ref IPathNode node, float magnitude, ConnectionType type = ConnectionType.None) : this(ref current, ref node)
        {
            this.connectionType = type;
            Magnitude = magnitude;
        }

        public NodeConnection(IPathNode node, ConnectionType type, float magnitude)
        {
            this.node = node;
            this.connectionType = type;
            Magnitude = magnitude;
        }

        public void Deconstruct(out IPathNode node,  out ConnectionType connection, out float magnitud)
        {
            node = this.node;
            connection = this.connectionType;
            magnitud = Magnitude;
        }

        private float DistanceBewtweenNodes(IPathNode a, IPathNode b)
        {
            return Vector3.Distance(a.Position, b.Position);
        }
    }

    [ExecuteInEditMode]
    public class MyNode : MonoBehaviour, IPathNode, ISplineNode
    {
        [SerializeField]
        public List<NodeConnection> nodeConnections = new List<NodeConnection>();
        [SerializeField] bool updateData = false;
        [HideInInspector]
        public uint idx = 0;
        public NodeData nodeData = null;
        public static CopyData copyData;
        private bool isSelected = false;
        private PatrolController patrol;

        public event Action<MyNode, MyNode> OnNodeAdded;
        public event Action<MyNode> OnNodeRemoved;

        public uint ID => idx;
        public Transform Transform { get => transform; }
        public List<NodeConnection> NodeConnections { get => nodeConnections; }
        public NodeData NodeData => nodeData;
        public bool IsSelected => isSelected;
        public Vector3 Position { get => transform.position; }
        public Action OnStart { get; set; }
        public PatrolController PatrolController { get => patrol; set => patrol = value; }

        #region Unity methods
        private void Awake()
        {
            if (!copyData.point) return;
            
            copyData.point.OnNodeAdded?.Invoke(copyData.point, this);
        }

        private void OnEnable()
        {
            if (!copyData.point) return;
            
            copyData.point.OnNodeAdded?.Invoke(copyData.point, this);
        }

        private void OnDisable()
        {
            OnNodeRemoved?.Invoke(this);
        }

        private void OnDrawGizmosSelected()
        {
            Select();


            foreach (var item in nodeConnections)
            {
                if (item.connectionType == ConnectionType.BIDIMENSIONAL)
                    Debug.DrawRay(transform.position, item.node.Position - transform.position, Color.blue);
            }

            if (NodeData != null && nodeData.ShouldDraw)
            {
                Gizmos.color = nodeData.NodeColor;
                Gizmos.DrawSphere(transform.localPosition, (float)nodeData.Radius); 
            }
        }
        #endregion

        public void ClearConnections()
        {
            nodeConnections.Clear();
        }

        public float GetDistanceBetweenNodes(in NodeConnection connection)
        {
            Vector3 value = connection.node.Position - transform.position;
            return value.magnitude;
        }

        public void SetIndex(uint idx) => this.idx = idx;

        public void SetNodeData(NodeData nodeData)
        {
            this.nodeData = new NodeData(nodeData);
        }

        private void Select()
        {
            if (!gameObject.activeSelf) return;

            copyData = new CopyData(true, this);
            isSelected = true;
        }
    }
}
