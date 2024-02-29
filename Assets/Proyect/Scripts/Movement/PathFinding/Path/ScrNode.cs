using Burmuruk.WorldG.Patrol;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.AI
{
    public class ScrNode : MonoBehaviour, IPathNode
    {
        public GameObject Item { get; set; }
        List<NodeConnection> nodeConnections;
        uint id;

        public uint ID => id;

        public Vector3 Position => Item ? Item.transform.position : Vector3.zero;

        public List<NodeConnection> NodeConnections => nodeConnections;

        private void OnDrawGizmosSelected()
        {
            foreach (var item in NodeConnections)
            {
                if (item.connectionType == ConnectionType.BIDIMENSIONAL)
                    Debug.DrawRay(transform.position, item.node.Position - transform.position, Color.blue);
            }
        }

        public void ClearConnections()
        {
            throw new System.NotImplementedException();
        }

        public float GetDistanceBetweenNodes(in NodeConnection connection)
        {
            throw new System.NotImplementedException();
        }

        public void SetIndex(uint idx)
        {
            id = idx;
        }
    }
}