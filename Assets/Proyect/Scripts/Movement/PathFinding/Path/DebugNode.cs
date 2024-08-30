using Burmuruk.WorldG.Patrol;
using UnityEngine;

namespace Burmuruk.AI
{
    public class DebugNode : MonoBehaviour
    {
        [SerializeField] ScrNode node;

        public ref ScrNode GetNode() => ref node;

        public void SetNode(in ScrNode node)
        {
            this.node = node;
        }

        private void OnDrawGizmosSelected()
        {
            if (node.NodeConnections == null || node.NodeConnections.Count <= 0) return;

            foreach (var connection in node.NodeConnections)
            {
                if (connection.connectionType == ConnectionType.BIDIMENSIONAL)
                    Debug.DrawRay(node.Position, connection.node.Position - node.Position, Color.blue);
            }
        }
    }
}
