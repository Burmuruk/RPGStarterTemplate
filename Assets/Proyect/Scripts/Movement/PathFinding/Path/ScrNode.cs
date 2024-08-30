using Burmuruk.WorldG.Patrol;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.AI
{
    [Serializable]
    public struct ScrNode : IPathNode
    {
        [SerializeField]
        List<NodeConnection> nodeConnections;
        [SerializeField] Vector3 position;
        [SerializeField] uint id;

        public uint ID => id;

        public Vector3 Position { get => position; }

        public List<NodeConnection> NodeConnections => nodeConnections;

        public ScrNode(uint idx, Vector3 position = default)
        {
            id = idx;
            nodeConnections = new();
            this.position = position;
        }

        //private void OnDrawGizmosSelected()
        //{
        //    if (nodeConnections == null) return;

        //    foreach (var itemType in NodeConnections)
        //    {
        //        if (itemType.connectionType == ConnectionType.BIDIMENSIONAL)
        //            Debug.DrawRay(transform.position, itemType.node2.Position - transform.position, Color.blue);
        //    }
        //}

        public void SetConnections(List<NodeConnection> connections)
        {
            nodeConnections = connections;
        }

        public void ClearConnections()
        {
            nodeConnections.Clear();
        }

        public float GetDistanceBetweenNodes(in NodeConnection connection)
        {
            Vector3 value = connection.node.Position - Position;
            return value.magnitude;
        }

        public void SetIndex(uint idx) => id = idx;
    }
}