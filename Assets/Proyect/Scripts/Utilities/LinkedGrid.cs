using Burmuruk.AI;
using Burmuruk.WorldG.Patrol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Burmuruk.Collections
{
    public unsafe class LinkedGrid<T> :IEnumerable<T> where T : struct, IPathNode
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

        public void Add(ref T item, int gap)
        {
            var node = new LinkedGridNode<T>(ref item, GetRowIdx(gap));

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

                while (i <= last.rowIdx)
                {
                    if (curNode.rowIdx == last.rowIdx)
                    {
                        founded = true;
                        break;
                    }

                    i += curNode[Direction.Next].rowIdx - curNode.rowIdx;

                    curNode = curNode[Direction.Next];
                }

                if (!founded) return;

                last[Direction.Left] = curNode;
                curNode[Direction.Right] = last;
            }
        }

        private void TryAddHeader()
        {
            if (Last.rowIdx == 0)
            {
                Headers.Add(last);
            }
        }

        private int GetRowIdx(int idx)
        {
            if (count == 123)
            {
                Debug.Print("hi");
            }
            if (Last == null)
            {
                return idx >= RowsCount ? (RowsCount - idx) : idx;
            }
            else if (idx <= last.rowIdx)
            {
                return idx;
            }
            else if (idx < RowsCount)
            {
                return idx;
            }

            return idx - Last.rowIdx;
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

        //public void AddAfter(LinkedGridNode<T> node, LinkedGridNode<T> newNode)
        //{
        //    newNode[Direction.Next] = node[Direction.Next];
        //    node[Direction.Next] = newNode;
        //    newNode[Direction.Previous] = node;

        //    count++;

        //    TryAddHeader(node);
        //    CreateSideConnections(node);
        //}

        //public LinkedGridNode<T> AddAfter(LinkedGridNode<T> node, ref T value)
        //{
        //    throw new NotImplementedException();
        //}
        //public void AddBefore(LinkedGridNode<T> node, LinkedGridNode<T> newNode)
        //{
        //    newNode[Direction.Next] = node;
        //    newNode[Direction.Previous] = node[Direction.Previous];
        //    node[Direction.Previous] = newNode;

        //    count++;
        //    TryAddHeader(node);
        //    CreateSideConnections(node);
        //}
        //public LinkedGridNode<T> AddBefore(LinkedGridNode<T> node, ref T value)
        //{
        //    throw new NotImplementedException();
        //}
        public LinkedGridNode<T> AddUp(LinkedGridNode<T> node, ref T value)
        {
            node[Direction.Up] = new LinkedGridNode<T>(ref value);

            verticalNodesCount++;

            return node[Direction.Up];
        }
        public LinkedGridNode<T> AddDown(LinkedGridNode<T> node, ref T value)
        {
            node[Direction.Down] = new LinkedGridNode<T>(ref value);

            verticalNodesCount++;

            return node[Direction.Down];
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

        public IEnumerator GetEnumerator()
        {
            return new LinkedGridEnumerator<LinkedGridNode<T>, T>(First);
        }

        public bool Remove(T item)
        {
            LinkedGridNode<T> node = First;
            while (node != Last)
            {
                if (node.Node.Position == item.Position)
                {
                    if (node[Direction.Previous] != null)
                    {
                        node[Direction.Previous][Direction.Next] = node[Direction.Next];
                        node[Direction.Next][Direction.Previous] = node[Direction.Previous];
                        return true;
                    }
                }

                node = node[Direction.Next];
            }

            return false;
        }

        public IPathNode[][][] ToArray()
        {
            if (Count <= 0) return null;

            IPathNode[][][] connections = new IPathNode[count][][];
            int columnSize = 0;
            int x = -1;
            var node = First;

            try
            {
                for (int y = -1; node != null; y++)
                {
                    var curNode = node;

                    if (curNode == Headers[x])
                    {
                        try
                        {
                            if (Headers.Count > x)
                                columnSize = checked((int)(Headers[x + 1].Node.ID - 1 - Headers[x].Node.ID));
                            else
                                columnSize = checked((int)(Count - Headers[x].Node.ID - 1));
                        }
                        catch (OverflowException)
                        {
                            throw new OverflowException();
                        }

                        connections[x] = new IPathNode[columnSize][];
                        ++x;
                    }

                    for (int z = 0; curNode != null; z++)
                    {
                        connections[x][y][z] = node.GetNodeCopy();
                        curNode = curNode[Direction.Down];
                    }

                    node = node[Direction.Next];
                    ++y;
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
            return (IEnumerator<T>)new LinkedGridEnumerator<LinkedGridNode<T>, T>(First);
        }
    }

    public class LinkedGridEnumerator<T, U> : IEnumerator<T> where T: LinkedGridNode<U> where U: struct, IPathNode
    {
        T current;

        public LinkedGridEnumerator(T first)
        {
            current = (T)new LinkedGridNode<U>();
            current[Direction.Next] = first;
        }

        public T Current => current;

        object IEnumerator.Current => current;

        public void Dispose()
        {
            current = null;
        } 

        public bool MoveNext()
        {
            if (current[Direction.Next] == null) return false;

            current = (T)current[Direction.Next]; 
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

    public class LinkedGridNode<T> where T : struct, IPathNode
    {
        Dictionary<Direction, LinkedGridNode<T>> connections = new();
        T node;
        int gapSize;

        public LinkedGridNode()
        {

        }

        public LinkedGridNode(ref T node, int gapSize = 0)
        {
            Node = node;
            this.gapSize = gapSize;
        }

        public ref T Node { get => ref node; }
        public int rowIdx { get => gapSize; }
        public Dictionary<Direction, LinkedGridNode<T>> Connections { get => connections; }

        public LinkedGridNode(LinkedGridNode<T> previous, ref T node)
        {
            connections = new()
            {
                { Direction.Previous, previous }
            };
            this.node = node;
        }

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
            }
        }

        public T GetNodeCopy()
        {
            return node;
        }
    }
}
