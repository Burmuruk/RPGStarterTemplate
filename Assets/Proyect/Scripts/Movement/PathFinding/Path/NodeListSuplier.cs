using System.Collections.Generic;
using UnityEngine;
using Burmuruk.AI;
using Burmuruk.Collections;
using System;

namespace Burmuruk.WorldG.Patrol
{
    [Serializable]
    public class NodeListSuplier : INodeListSupplier
    {
        private Vector3 startNode = default;
        private Vector3 endNode = default;
        ICollection<IPathNode> nodes = null;
        IPathNode[][][] connections;
        private float maxDistance = 2;
        private float maxAngle = 5;
        float pRadious = .5f;
        float height = 1;

        public pState connectionsState = pState.None;

        public Vector3 StartNode => startNode;

        public Vector3 EndNode => endNode;

        public IEnumerable<IPathNode> Nodes => nodes;

        #region Public methods
        public void CalculateNodesConnections()
        {
            throw new NotImplementedException();
        }

        public void Clear() => connections = null;

        public void ClearNodeConnections()
        {
            if (connections == null) return;

            connectionsState = pState.deleting;

            foreach (var node in nodes)
                node.ClearConnections();

            connectionsState = pState.None;
        }

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
                    length = connections[i].Length;

                    for (int j = 0; j < connections[i].Length; j++)
                    {
                        if (i == length ||
                            (start.y >= connections[i][j][0].Position.x && start.x < connections[i][j + 1][0].Position.y))
                        {
                            length = connections[i][j].Length;

                            for (int k = 0; k < connections[i][j].Length; k++)
                            {
                                if (i == length ||
                                    (start.z >= connections[i][j][k].Position.z && start.x < connections[i][j][k + 1].Position.z))
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

        public void SetTarget(IPathNode[] nodes, float pRadious = .2f, float maxDistance = 2, float maxAngle = 45, float height = 1)
        {
            this.nodes = nodes;
            this.pRadious = pRadious;
            this.maxDistance = maxDistance;
            this.maxAngle = maxAngle;
            this.height = height;
        } 

        public void SetNodes(ICollection<IPathNode> nodes) =>
            this.nodes = nodes;

        public void SetConnections(IPathNode[][][] connections)
        {
            this.connections = connections;
        }

        #endregion
    }
}
