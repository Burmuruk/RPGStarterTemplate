using Burmuruk.AI;
using Burmuruk.WorldG.Patrol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Collections
{
    public class LinkedGrid<T> :IEnumerable<T> where T : IPathNode
    {
        int count;
        int verticalNodesCount;
        LinkedGridNode<T> first;
        LinkedGridNode<T> last;

        public LinkedGrid(int rows)
        {
            this.RowsCount = rows;
        }

        public List<LinkedGridNode<T>> Headers { get; private set; } = new();
        public int RowsCount { get; private set; }
        public ref LinkedGridNode<T> First { get => ref first; }
        public ref  LinkedGridNode<T> Last { get => ref last; }
        public int Count => count;
        public int DeepCount {  get => verticalNodesCount + count; }

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(T item, int rowIdx, int columnIdx)
        {
            LinkedGridNode<T> node = new LinkedGridNode<T>(ref item, GetRowIdx(rowIdx, columnIdx), columnIdx);

            if (First != null)
            {
                node[Direction.Previous] = Last;
                Last[Direction.Next] = node;
                Last = node;
            }
            else
            {
                First = node;
                Last = node;
                Headers.Add(last);
            }

            count++;

            TryAddHeader();
            CreateSideConnections();
        }

        private void CreateSideConnections()
        {
            if (Headers.Count > 1)
            {
                int headerIdx = Headers.Count - 2;

                var curNode = Headers[headerIdx];
                int i = 0;
                bool founded = false;

                while (i <= last.RowIdx)
                {
                    if (curNode.RowIdx == last.RowIdx)
                    {
                        founded = true;
                        break;
                    }

                    i += curNode[Direction.Next].RowIdx - curNode.RowIdx;

                    curNode = curNode[Direction.Next];
                }

                if (!founded) return;

                last[Direction.Left] = curNode;
                curNode[Direction.Right] = last;
            }
        }

        private void TryAddHeader()
        {
            if (last[Direction.Previous] != null && last.ColumnIdx > last[Direction.Previous].ColumnIdx)
            {
                Headers.Add(last);
            }
        }

        private int GetRowIdx(int yIdx, int xIdx)
        {
            if (last != null && (last.ColumnIdx - xIdx > 1 || (last.ColumnIdx == xIdx && yIdx <= last.RowIdx)))
            {
                throw new InvalidOperationException();
            }

            if (last == null)
            {
                return yIdx >= RowsCount ? (RowsCount - yIdx) : yIdx;
            }
            else if (yIdx < RowsCount)
            {
                return yIdx;
            }

            return yIdx - last.RowIdx;
        }

        //private void MoveHeaders(int spaces)
        //{
        //    for (int i = 0; i < Headers.Count; ++i)
        //    {
        //        for (int j = 0; j < spaces; ++j)
        //        {
        //            Headers[i] = spaces > 0 ? Headers[i][Direction.Next] : Headers[i][Direction.Previous];
        //        }
        //    }
        //}

        //public void AddAfter(LinkedGridNode<T> node2, LinkedGridNode<T> newNode)
        //{
        //    newNode[Direction.Next] = node2[Direction.Next];
        //    node2[Direction.Next] = newNode;
        //    newNode[Direction.Previous] = node2;

        //    count++;

        //    TryAddHeader(node2);
        //    CreateSideConnections(node2);
        //}

        //public LinkedGridNode<T> AddAfter(LinkedGridNode<T> node2, ref T ability)
        //{
        //    throw new NotImplementedException();
        //}
        //public void AddBefore(LinkedGridNode<T> node2, LinkedGridNode<T> newNode)
        //{
        //    newNode[Direction.Next] = node2;
        //    newNode[Direction.Previous] = node2[Direction.Previous];
        //    node2[Direction.Previous] = newNode;

        //    count++;
        //    TryAddHeader(node2);
        //    CreateSideConnections(node2);
        //}
        //public LinkedGridNode<T> AddBefore(LinkedGridNode<T> node2, ref T ability)
        //{
        //    throw new NotImplementedException();
        //}
        public LinkedGridNode<T> AddUp(LinkedGridNode<T> node, T value)
        {
            return AddVerticalNode(node, Direction.Up, ref value);
        }
        public LinkedGridNode<T> AddDown(LinkedGridNode<T> node, ref T value)
        {
            return AddVerticalNode(node, Direction.Down, ref value);
        }

        private LinkedGridNode<T> AddVerticalNode(LinkedGridNode<T> node, Direction direction, ref T value)
        {
            LinkedGridNode<T> nodeAbove = node;

            while (nodeAbove[direction] != null)
            {
                nodeAbove = nodeAbove[direction];
            }

            nodeAbove[direction] = new LinkedGridNode<T>(ref value, columnIdx: node.ColumnIdx);

            var oppisteDir = direction == Direction.Up ? Direction.Down : Direction.Up;
            nodeAbove[direction][oppisteDir] = nodeAbove;
            CopyConectionsToChild(node, nodeAbove[direction]);

            verticalNodesCount++;

            return nodeAbove[direction];
        }

        private void CopyConectionsToChild(LinkedGridNode<T> parent, LinkedGridNode<T> child)
        {
            foreach (var connection in parent.Connections)
            {
                if (connection.Key != Direction.Up && connection.Key != Direction.Down)
                {
                    child.Connections.Add(connection.Key, connection.Value);
                }
            }
        }

        public void AddFirst(LinkedGridNode<T> node)
        {
            node[Direction.Next] = First;
            First[Direction.Previous] = node;
            First = node;

            count++;
            TryAddHeader();
            CreateSideConnections();
        }
        public LinkedGridNode<T> AddFirst(T value)
        {
            throw new NotImplementedException();
        }
        public void AddLast(LinkedGridNode<T> node)
        {
            node[Direction.Previous] = Last;
            Last[Direction.Next] = node;
            Last = node;

            count++;
            TryAddHeader();
            CreateSideConnections();
        }
        public LinkedGridNode<T> AddLast(T value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            First = null; Last = null;
        }

        public bool Contains(T item)
        {
            LinkedGridNode<T> node = First;
            while (node != Last)
            {
                if (node.Node.Position == item.Position)
                {
                    return true;
                }

                node = node[Direction.Next];
            }

            return false;
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            LinkedGridNode<T> node = First;
            while (node != Last)
            {
                if (node.Node.ID == item.ID)
                {
                    if (node[Direction.Previous] != null)
                    {
                        if (node[Direction.Previous][Direction.Next] == node)
                        {
                            if (node[Direction.Up] != null)
                            {
                                node[Direction.Previous][Direction.Next] = node[Direction.Up];
                            }
                            else
                            {
                                node[Direction.Previous][Direction.Next] = node[Direction.Next];
                            }
                        }
                    }

                    if (node[Direction.Next] != null)
                    {
                        if (node[Direction.Next][Direction.Previous] == node)
                        {
                            if (node[Direction.Up] != null)
                            {
                                node[Direction.Next][Direction.Previous] = node[Direction.Up];
                            }
                            else
                            {
                                node[Direction.Next][Direction.Previous] = node[Direction.Previous];
                            }
                        }
                    }

                    if (node[Direction.Down] != null)
                    {
                        node[Direction.Down][Direction.Up] = node[Direction.Up];
                    }

                    if (node[Direction.Up] != null)
                    {
                        node[Direction.Up][Direction.Down] = node[Direction.Down];
                    }

                    return true;
                }

                node = node[Direction.Next];
            }

            return false;
        }

        public bool Remove(LinkedGridNode<T> node)
        {
            if (node[Direction.Previous] != null)
            {
                node[Direction.Previous][Direction.Next] = node[Direction.Next];
            }

            if (node[Direction.Next] != null)
            {
                node[Direction.Next][Direction.Previous] = node[Direction.Previous];
            }

            if (node[Direction.Down] != null)
            {
                node[Direction.Down][Direction.Up] = node[Direction.Up];
            }

            if (node[Direction.Up] != null)
            {
                node[Direction.Up][Direction.Down] = node[Direction.Down];
            }

            return true;
        }

        public IPathNode[][][] ToArray()
        {
            if (Count <= 0) return null;

            IPathNode[][][] connections = new IPathNode[count][][];
            int columnSize = 0;
            int x = -1;
            var node2 = First;

            try
            {
                for (int y = 0; node2 != null; y++)
                {
                    var curNode = node2;

                    if (x + 1 < Headers.Count && curNode == Headers[x + 1])
                    {
                        try
                        {
                            if (Headers.Count > x + 2)
                                columnSize = checked((int)(Headers[x + 2].Node.ID - Headers[x + 1].Node.ID));
                            else
                                columnSize = checked((int)(last.ID - Headers[x + 1].Node.ID + 1));
                        }
                        catch (OverflowException)
                        {
                            throw new OverflowException();
                        }

                        connections[x + 1] = new IPathNode[columnSize][];
                        ++x;
                        y = 0;
                    }

                    List<IPathNode> zNodes = null;
                    for (int z = 0; curNode != null; z++)
                    {
                        zNodes ??= new();
                        zNodes.Add(node2.GetNodeCopy());
                        curNode = curNode[Direction.Down];
                    }
                    connections[x][y] = zNodes.ToArray();

                    node2 = node2[Direction.Next];
                }
            }
            catch (ArgumentOutOfRangeException)
            {

                throw;
            }

            return connections;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new LinkedGridEnumerator<T>(First);
        }

        public IEnumerator GetEnumerator()
        {
            return new LinkedGridEnumerator<T>(First);
        }
    }

    public class LinkedGridEnumerator<T> : IEnumerator<T> where T: IPathNode
    {
        LinkedGridNode<T> current;

        public LinkedGridEnumerator(LinkedGridNode<T> first)
        {
            current = new LinkedGridNode<T>();
            current[Direction.Next] = first;
        }

        public T Current => current.Node;
        public LinkedGridNode<T> CurrentLinkedNode => current;

        object IEnumerator.Current => current;

        public void Dispose()
        {
            current = null;
        } 

        public bool MoveNext()
        {
            if (current[Direction.Next] == null) return false;

            current = current[Direction.Next]; 
            return true;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
    public enum Direction
    {
        None,
        Previous,
        Next,
        Up,
        Down,
        Right,
        Left,
    }

    public class LinkedGridNode<T> where T : IPathNode
    {
        Dictionary<Direction, LinkedGridNode<T>> connections = new();
        T node;
        int gapSize;

        public ref T Node { get => ref node; }
        public int RowIdx { get => gapSize; }
        public int ColumnIdx { get; private set; }
        public uint ID { get =>  node.ID; }
        public LinkedGridNode<T> this[Direction d]
        {
            get => connections.ContainsKey(d) ? connections[d] : null;
            set
            {
                if (connections.ContainsKey(d))
                {
                    connections[d] = value;
                }
                else
                {
                    connections.Add(d, value);
                }

                if (d == Direction.Next || d == Direction.Left || d == Direction.Right)
                {
                    if (connections.ContainsKey(Direction.Up))
                    {
                        try
                        {
                            connections[Direction.Up][d] = value;
                        }
                        catch (NullReferenceException)
                        {

                            throw;
                        }
                    }
                    try
                    {
                    }
                    catch (NullReferenceException)
                    {

                        throw;
                    }
                }
                try
                {
                }
                catch (NullReferenceException)
                {

                    throw;
                }
            }
        }

        public LinkedGridNode()
        {

        }

        public LinkedGridNode(ref T node, int gapSize = 0, int columnIdx = 0)
        {
            Node = node;
            this.gapSize = gapSize;
            ColumnIdx = columnIdx;
        }

        public LinkedGridNode(LinkedGridNode<T> previous, ref T node)
        {
            connections = new()
            {
                { Direction.Previous, previous }
            };
            this.node = node;
        }

        public Dictionary<Direction, LinkedGridNode<T>> Connections { get => connections; }

        public T GetNodeCopy()
        {
            return node;
        }
    }
}