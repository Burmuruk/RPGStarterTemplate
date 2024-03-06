using Burmuruk.WorldG.Patrol;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.AI
{
    public class ScrNode : MonoBehaviour, IPathNode
    {
        public GameObject Item { get; set; }
        [SerializeField]
        List<NodeConnection> nodeConnections = new();
        uint id;

        public uint ID => id;

        public Vector3 Position => transform.position;

        public List<NodeConnection> NodeConnections => nodeConnections;

        //private void OnDrawGizmosSelected()
        //{
        //    if (nodeConnections == null) return;

        //    foreach (var item in NodeConnections)
        //    {
        //        if (item.connectionType == ConnectionType.BIDIMENSIONAL)
        //            Debug.DrawRay(transform.position, item.node.Position - transform.position, Color.blue);
        //    }
        //}

        public void ClearConnections()
        {
            nodeConnections.Clear();
        }

        public float GetDistanceBetweenNodes(in NodeConnection connection)
        {
            Vector3 value = connection.node.Position - transform.position;
            return value.magnitude;
        }

        public void SetIndex(uint idx) => id = idx;
    }
}