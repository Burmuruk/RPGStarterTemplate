using Burmuruk.AI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Burmuruk.Collections
{
    public class LinkedGrid<T> : ICollection<T>, IEnumerable<T> where T : class
    {
        List<LinkedGridNode<T>> headers = new();
        int rows;
        int count;

        public LinkedGrid(int rows)
        {
            this.rows = rows;
        }

        public LinkedGridNode<T> First { get; private set; }
        public LinkedGridNode<T> Last { get; private set; }
        public int Count => count;

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(T item)
        {
            var node = new LinkedGridNode<T>(item);

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
                headers.Add(node);
            }

            count++;

            TryAddHeader(node);
            CreateSideConnections(node);
        }

        private void CreateSideConnections(LinkedGridNode<T> node)
        {
            if (count > rows)
            {
                int headerIdx = (count - 1) / rows;

                int idx = count - rows * (headerIdx <= 0 ? 1 : headerIdx);
                int i = 0;
                var curNode = headers[headerIdx - 1];

                while (++i < idx)
                {
                    curNode = curNode[Direction.Next];
                }

                node[Direction.Left] = curNode;
                curNode[Direction.Right] = node;
            }
        }

        private void TryAddHeader(LinkedGridNode<T> node)
        {
            if (count <= rows) return;

            if ((float)count % (float)rows == 1)
            {
                headers.Add(node);
            }
        }

        private void MoveHeaders(int spaces)
        {
            for (int i = 0; i < headers.Count; ++i)
            {
                for (int j = 0; j < spaces; ++j)
                {
                    headers[i] = spaces > 0 ? headers[i][Direction.Next] : headers[i][Direction.Previous];
                }
            }
        }

        public void AddAfter(LinkedGridNode<T> node, LinkedGridNode<T> newNode)
        {
            newNode[Direction.Next] = node[Direction.Next];
            node[Direction.Next] = newNode;
            newNode[Direction.Previous] = node;

            count++;

            TryAddHeader(node);
            CreateSideConnections(node);
        }

        public LinkedGridNode<T> AddAfter(LinkedGridNode<T> node, T value)
        {
            throw new NotImplementedException();
        }
        public void AddBefore(LinkedGridNode<T> node, LinkedGridNode<T> newNode)
        {
            newNode[Direction.Next] = node;
            newNode[Direction.Previous] = node[Direction.Previous];
            node[Direction.Previous] = newNode;

            count++;
            TryAddHeader(node);
            CreateSideConnections(node);
        }
        public LinkedGridNode<T> AddBefore(LinkedGridNode<T> node, T value)
        {
            throw new NotImplementedException();
        }
        public LinkedGridNode<T> AddUp(LinkedGridNode<T> node, T value)
        {
            node[Direction.Up] = new LinkedGridNode<T>(value);
            return node[Direction.Up];
        }
        public LinkedGridNode<T> AddDown(LinkedGridNode<T> node, T value)
        {
            node[Direction.Down] = new LinkedGridNode<T>(value);
            return node[Direction.Down];
        }
        public void AddFirst(LinkedGridNode<T> node)
        {
            node[Direction.Next] = First;
            First[Direction.Previous] = node;
            First = node;

            count++;
            TryAddHeader(node);
            CreateSideConnections(node);
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
            TryAddHeader(node);
            CreateSideConnections(node);
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
                if (node.Node == item)
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
                if (node.Node == item)
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

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return (IEnumerator<T>)new LinkedGridEnumerator<LinkedGridNode<T>, T>(First);
        }
    }

    public class LinkedGridEnumerator<T, U> : IEnumerator<T> where T: LinkedGridNode<U>
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
            throw new NotImplementedException();
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

    public class LinkedGridNode<T>
    {
        Dictionary<Direction, LinkedGridNode<T>> connections = new();

        public LinkedGridNode()
        {

        }

        public LinkedGridNode(T node)
        {
            Node = node;
        }

        public LinkedGridNode(LinkedGridNode<T> previous, T node)
        {
            connections = new()
            {
                { Direction.Previous, previous }
            };
            Node = node;
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

        public T Node { get; private set; }
        public Dictionary<Direction, LinkedGridNode<T>> Connections { get => connections; }
    }
}
