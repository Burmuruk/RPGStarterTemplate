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

        //public LinkedGridNode<T> AddAfter(LinkedGridNode<T> node2, ref T value)
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
        //public LinkedGridNode<T> AddBefore(LinkedGridNode<T> node2, ref T value)
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

            nodeAbove[direction] = new LinkedGridNode<T>(ref value);

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
                        connections[Direction.Up][d] = value;
                    }
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

#region without variance
//using Burmuruk.AI;
//using Burmuruk.WorldG.Patrol;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace Burmuruk.Collections
//{
//    public class LinkedGrid<T> : IEnumerable<T> where T : IPathNode
//    {
//        int count;
//        int verticalNodesCount;
//        LinkedGridNode<T> first;
//        LinkedGridNode<T> last;

//        public LinkedGrid(int rows)
//        {
//            this.RowsCount = rows;
//        }

//        public List<LinkedGridNode<T>> Headers { get; private set; } = new();
//        public int RowsCount { get; private set; }
//        public ref LinkedGridNode<T> First { get => ref first; }
//        public ref LinkedGridNode<T> Last { get => ref last; }
//        public int Count => count;
//        public int DeepCount { get => verticalNodesCount + count; }

//        public bool IsReadOnly => throw new NotImplementedException();

//        public void Add(ref T item, int rowIdx, int columnIdx)
//        {
//            LinkedGridNode<T> node2 = new LinkedGridNode<T>(ref item, GetRowIdx(rowIdx, columnIdx), columnIdx);

//            if (First != null)
//            {
//                node2[Direction.Previous] = Last;
//                Last[Direction.Next] = node2;
//                Last = node2;
//            }
//            else
//            {
//                First = node2;
//                Last = node2;
//                Headers.Add(last);
//            }

//            count++;

//            TryAddHeader();
//            CreateSideConnections();
//        }

//        private void CreateSideConnections()
//        {
//            if (Headers.Count > 1)
//            {
//                int headerIdx = Headers.Count - 2;

//                var curNode = Headers[headerIdx];
//                int i = 0;
//                bool founded = false;

//                while (i <= last.RowIdx)
//                {
//                    if (curNode.RowIdx == last.RowIdx)
//                    {
//                        founded = true;
//                        break;
//                    }

//                    i += curNode[Direction.Next].RowIdx - curNode.RowIdx;

//                    curNode = curNode[Direction.Next];
//                }

//                if (!founded) return;

//                last[Direction.Left] = curNode;
//                curNode[Direction.Right] = last;
//            }
//        }

//        private void TryAddHeader()
//        {
//            if (last[Direction.Previous] != null && last.ColumnIdx > last[Direction.Previous].ColumnIdx)
//            {
//                Headers.Add(last);
//            }
//        }

//        private int GetRowIdx(int yIdx, int xIdx)
//        {
//            if (last != null && (last.ColumnIdx - xIdx > 1 || (last.ColumnIdx == xIdx && yIdx <= last.RowIdx)))
//            {
//                throw new InvalidOperationException();
//            }

//            if (last == null)
//            {
//                return yIdx >= RowsCount ? (RowsCount - yIdx) : yIdx;
//            }
//            else if (yIdx < RowsCount)
//            {
//                return yIdx;
//            }

//            return yIdx - last.RowIdx;
//        }

//        //private void MoveHeaders(int spaces)
//        //{
//        //    for (int i = 0; i < Headers.Count; ++i)
//        //    {
//        //        for (int j = 0; j < spaces; ++j)
//        //        {
//        //            Headers[i] = spaces > 0 ? Headers[i][Direction.Next] : Headers[i][Direction.Previous];
//        //        }
//        //    }
//        //}

//        //public void AddAfter(LinkedGridNode<T> node2, LinkedGridNode<T> newNode)
//        //{
//        //    newNode[Direction.Next] = node2[Direction.Next];
//        //    node2[Direction.Next] = newNode;
//        //    newNode[Direction.Previous] = node2;

//        //    count++;

//        //    TryAddHeader(node2);
//        //    CreateSideConnections(node2);
//        //}

//        //public LinkedGridNode<T> AddAfter(LinkedGridNode<T> node2, ref T value)
//        //{
//        //    throw new NotImplementedException();
//        //}
//        //public void AddBefore(LinkedGridNode<T> node2, LinkedGridNode<T> newNode)
//        //{
//        //    newNode[Direction.Next] = node2;
//        //    newNode[Direction.Previous] = node2[Direction.Previous];
//        //    node2[Direction.Previous] = newNode;

//        //    count++;
//        //    TryAddHeader(node2);
//        //    CreateSideConnections(node2);
//        //}
//        //public LinkedGridNode<T> AddBefore(LinkedGridNode<T> node2, ref T value)
//        //{
//        //    throw new NotImplementedException();
//        //}
//        public LinkedGridNode<T> AddUp(LinkedGridNode<T> node2, ref T value)
//        {
//            return AddVerticalNode(node2, Direction.Up, ref value);
//        }
//        public LinkedGridNode<T> AddDown(LinkedGridNode<T> node2, ref T value)
//        {
//            return AddVerticalNode(node2, Direction.Down, ref value);
//        }

//        private LinkedGridNode<T> AddVerticalNode(LinkedGridNode<T> node2, Direction direction, ref T value)
//        {
//            LinkedGridNode<T> nodeAbove = node2;

//            while (nodeAbove[direction] != null)
//            {
//                nodeAbove = nodeAbove[direction];
//            }

//            nodeAbove[direction] = new LinkedGridNode<T>(ref value);

//            var oppisteDir = direction == Direction.Up ? Direction.Down : Direction.Up;
//            nodeAbove[direction][oppisteDir] = nodeAbove;
//            CopyConectionsToChild(node2, nodeAbove[direction]);

//            verticalNodesCount++;

//            return nodeAbove[direction];
//        }

//        private void CopyConectionsToChild(LinkedGridNode<T> parent, LinkedGridNode<T> child)
//        {
//            foreach (var connection in parent.Connections)
//            {
//                if (connection.Key != Direction.Up && connection.Key != Direction.Down)
//                {
//                    child.Connections.Add(connection.Key, connection.Value);
//                }
//            }
//        }

//        public void AddFirst(LinkedGridNode<T> node2)
//        {
//            node2[Direction.Next] = First;
//            First[Direction.Previous] = node2;
//            First = node2;

//            count++;
//            TryAddHeader();
//            CreateSideConnections();
//        }
//        public LinkedGridNode<T> AddFirst(T value)
//        {
//            throw new NotImplementedException();
//        }
//        public void AddLast(LinkedGridNode<T> node2)
//        {
//            node2[Direction.Previous] = Last;
//            Last[Direction.Next] = node2;
//            Last = node2;

//            count++;
//            TryAddHeader();
//            CreateSideConnections();
//        }
//        public LinkedGridNode<T> AddLast(T value)
//        {
//            throw new NotImplementedException();
//        }

//        public void Clear()
//        {
//            First = null; Last = null;
//        }

//        public bool Contains(T item)
//        {
//            LinkedGridNode<T> node2 = First;
//            while (node2 != Last)
//            {
//                if (node2.Node.Position == item.Position)
//                {
//                    return true;
//                }

//                node2 = node2[Direction.Next];
//            }

//            return false;
//        }

//        public void CopyTo(Array array, int index)
//        {
//            throw new NotImplementedException();
//        }

//        public void CopyTo(T[] array, int arrayIndex)
//        {
//            throw new NotImplementedException();
//        }

//        public IEnumerator GetEnumerator()
//        {
//            return new LinkedGridEnumerator<LinkedGridNode<T>, T>(First);
//        }

//        public bool Remove(T item)
//        {
//            LinkedGridNode<T> node2 = First;
//            while (node2 != Last)
//            {
//                if (node2.Node.Position == item.Position)
//                {
//                    if (node2[Direction.Previous] != null)
//                    {
//                        node2[Direction.Previous][Direction.Next] = node2[Direction.Next];
//                        node2[Direction.Next][Direction.Previous] = node2[Direction.Previous];
//                        return true;
//                    }
//                }

//                node2 = node2[Direction.Next];
//            }

//            return false;
//        }

//        public IPathNode[][][] ToArray()
//        {
//            if (Count <= 0) return null;

//            IPathNode[][][] connections = new IPathNode[count][][];
//            int columnSize = 0;
//            int x = -1;
//            var node2 = First;

//            try
//            {
//                for (int y = 0; node2 != null; y++)
//                {
//                    var curNode = node2;

//                    if (x + 1 < Headers.Count && curNode == Headers[x + 1])
//                    {
//                        try
//                        {
//                            if (Headers.Count > x + 2)
//                                columnSize = checked((int)(Headers[x + 2].Node.ID - Headers[x + 1].Node.ID));
//                            else
//                                columnSize = checked((int)(last.ID - Headers[x + 1].Node.ID + 1));
//                        }
//                        catch (OverflowException)
//                        {
//                            throw new OverflowException();
//                        }

//                        connections[x + 1] = new IPathNode[columnSize][];
//                        ++x;
//                        y = 0;
//                    }

//                    List<IPathNode> zNodes = null;
//                    for (int z = 0; curNode != null; z++)
//                    {
//                        zNodes ??= new();
//                        zNodes.Add(node2.GetNodeCopy());
//                        curNode = curNode[Direction.Down];
//                    }
//                    connections[x][y] = zNodes.ToArray();

//                    node2 = node2[Direction.Next];
//                }
//            }
//            catch (ArgumentOutOfRangeException)
//            {

//                throw;
//            }

//            return connections;
//        }

//        IEnumerator<T> IEnumerable<T>.GetEnumerator()
//        {
//            return (IEnumerator<T>)new LinkedGridEnumerator<LinkedGridNode<T>, T>(First);
//        }
//    }

//    public class LinkedGridEnumerator<T, U> : IEnumerator<T> where T : LinkedGridNode<U> where U : IPathNode
//    {
//        T current;

//        public LinkedGridEnumerator(T first)
//        {
//            current = (T)new LinkedGridNode<U>();
//            current[Direction.Next] = first;
//        }

//        public T Current => current;

//        object IEnumerator.Current => current;

//        public void Dispose()
//        {
//            current = null;
//        }

//        public bool MoveNext()
//        {
//            if (current[Direction.Next] == null) return false;

//            current = (T)current[Direction.Next];
//            return true;
//        }

//        public void Reset()
//        {
//            throw new NotImplementedException();
//        }
//    }
//    public enum Direction
//    {
//        None,
//        Previous,
//        Next,
//        Up,
//        Down,
//        Right,
//        Left,
//    }

//    public class LinkedGridNode<T> where T : IPathNode
//    {
//        Dictionary<Direction, LinkedGridNode<T>> connections = new();
//        T node2;
//        int gapSize;

//        public ref T Node { get => ref node2; }
//        public int RowIdx { get => gapSize; }
//        public int ColumnIdx { get; private set; }
//        public uint ID { get => node2.ID; }
//        public LinkedGridNode<T> this[Direction d]
//        {
//            get => connections.ContainsKey(d) ? connections[d] : null;
//            set
//            {
//                if (connections.ContainsKey(d))
//                {
//                    connections[d] = value;
//                }
//                else
//                {
//                    connections.Add(d, value);
//                }

//                if (d == Direction.Next || d == Direction.Left || d == Direction.Right)
//                {
//                    if (connections.ContainsKey(Direction.Up))
//                    {
//                        connections[Direction.Up][d] = value;
//                    }
//                }
//            }
//        }

//        public LinkedGridNode()
//        {

//        }

//        public LinkedGridNode(ref T node2, int gapSize = 0, int columnIdx = 0)
//        {
//            Node = node2;
//            this.gapSize = gapSize;
//            ColumnIdx = columnIdx;
//        }

//        public LinkedGridNode(LinkedGridNode<T> previous, ref T node2)
//        {
//            connections = new()
//            {
//                { Direction.Previous, previous }
//            };
//            this.node2 = node2;
//        }

//        public Dictionary<Direction, LinkedGridNode<T>> Connections { get => connections; }

//        public T GetNodeCopy()
//        {
//            return node2;
//        }
//    }
//} 
#endregion

#region Variance
//using Burmuruk.AI;
//using Burmuruk.WorldG.Patrol;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEditor.Experimental.GraphView;
//using static UnityEngine.Rendering.DebugUI;

//namespace Burmuruk.Collections
//{
//    public interface ILinkedGrid<out T> where T : IPathNode
//    {
//        public ILinkedGridNode<T> First { get; }
//        public ILinkedGridNode<T> Last { get; }
//        //public ILinkedGridNode<T> AddUp(ILinkedGridNode<T> node2, T value);
//        //public ILinkedGridNode<T> AddDown(ILinkedGridNode<T> node2, T value);
//    }

//    public interface ILinkedGridNode<out T> where T : IPathNode
//    {
//        public T Node { get; }
//        public T GetNodeCopy();
//        public ILinkedGridNode<T> GetDir(Direction d);
//        public int RowIdx { get; }
//        public int ColumnIdx { get; }
//        public uint ID { get; }
//    }

//    public interface ILinkedGridNodeIN<in T> where T : IPathNode
//    {
//        public void SetDir(Direction d, ILinkedGridNode<T> node2);
//    }

//    public interface ILinkedEnumeratorIn<in T, out U> where T : ILinkedGridNode<U> where U : IPathNode
//    {
//        public void Initilize(T first);
//    }

//    public interface ILinkedGridEnumerator<out T, out U> where T : ILinkedGridNode<U> where U : IPathNode
//    {
//        public T Current { get; }
//    }

//    public class LinkedGrid<T> : ILinkedGrid<T>, IEnumerable<T> where T : IPathNode
//    {
//        int count;
//        int verticalNodesCount;
//        LinkedGridNode<T> first;
//        LinkedGridNode<T> last;

//        public LinkedGrid(int rows)
//        {
//            this.RowsCount = rows;
//        }

//        public List<ILinkedGridNode<T>> Headers { get; private set; } = new();
//        public int RowsCount { get; private set; }
//        public ILinkedGridNode<T> First { get => first; }
//        public ILinkedGridNode<T> Last { get => last; }
//        public int Count => count;
//        public int DeepCount { get => verticalNodesCount + count; }

//        public bool IsReadOnly => throw new NotImplementedException();

//        public void Add(ref T item, int rowIdx, int columnIdx)
//        {
//            LinkedGridNode<T> node2 = new LinkedGridNode<T>(ref item, GetRowIdx(rowIdx, columnIdx), columnIdx);

//            if (First != null)
//            {
//                node2[Direction.Previous] = Last;
//                Last[Direction.Next] = node2;
//                last = node2;
//            }
//            else
//            {
//                first = node2;
//                last = node2;
//                Headers.Add(last);
//            }

//            count++;

//            TryAddHeader();
//            CreateSideConnections();
//        }

//        private void CreateSideConnections()
//        {
//            if (Headers.Count > 1)
//            {
//                int headerIdx = Headers.Count - 2;

//                var curNode = Headers[headerIdx];
//                int i = 0;
//                bool founded = false;

//                while (i <= last.RowIdx)
//                {
//                    if (curNode.RowIdx == last.RowIdx)
//                    {
//                        founded = true;
//                        break;
//                    }

//                    i += curNode[Direction.Next].RowIdx - curNode.RowIdx;

//                    curNode = curNode[Direction.Next];
//                }

//                if (!founded) return;

//                last[Direction.Left] = curNode;
//                curNode[Direction.Right] = last;
//            }
//        }

//        private void TryAddHeader()
//        {
//            if (last[Direction.Previous] != null && last.ColumnIdx > last[Direction.Previous].ColumnIdx)
//            {
//                Headers.Add(last);
//            }
//        }

//        private int GetRowIdx(int yIdx, int xIdx)
//        {
//            if (last != null && (last.ColumnIdx - xIdx > 1 || (last.ColumnIdx == xIdx && yIdx <= last.RowIdx)))
//            {
//                throw new InvalidOperationException();
//            }

//            if (last == null)
//            {
//                return yIdx >= RowsCount ? (RowsCount - yIdx) : yIdx;
//            }
//            else if (yIdx < RowsCount)
//            {
//                return yIdx;
//            }

//            return yIdx - last.RowIdx;
//        }

//        //private void MoveHeaders(int spaces)
//        //{
//        //    for (int i = 0; i < Headers.Count; ++i)
//        //    {
//        //        for (int j = 0; j < spaces; ++j)
//        //        {
//        //            Headers[i] = spaces > 0 ? Headers[i][Direction.Next] : Headers[i][Direction.Previous];
//        //        }
//        //    }
//        //}

//        //public void AddAfter(LinkedGridNode<T> node2, LinkedGridNode<T> newNode)
//        //{
//        //    newNode[Direction.Next] = node2[Direction.Next];
//        //    node2[Direction.Next] = newNode;
//        //    newNode[Direction.Previous] = node2;

//        //    count++;

//        //    TryAddHeader(node2);
//        //    CreateSideConnections(node2);
//        //}

//        //public LinkedGridNode<T> AddAfter(LinkedGridNode<T> node2, ref T value)
//        //{
//        //    throw new NotImplementedException();
//        //}
//        //public void AddBefore(LinkedGridNode<T> node2, LinkedGridNode<T> newNode)
//        //{
//        //    newNode[Direction.Next] = node2;
//        //    newNode[Direction.Previous] = node2[Direction.Previous];
//        //    node2[Direction.Previous] = newNode;

//        //    count++;
//        //    TryAddHeader(node2);
//        //    CreateSideConnections(node2);
//        //}
//        //public LinkedGridNode<T> AddBefore(LinkedGridNode<T> node2, ref T value)
//        //{
//        //    throw new NotImplementedException();
//        //}
//        public ILinkedGridNode<T> AddUp(ILinkedGridNode<T> node2, ref T value)
//        {
//            return AddVerticalNode(node2, Direction.Up, ref value);
//        }
//        public ILinkedGridNode<T> AddDown(LinkedGridNode<T> node2, ref T value)
//        {
//            return AddVerticalNode(node2, Direction.Down, ref value);
//        }

//        private ILinkedGridNode<T> AddVerticalNode(ILinkedGridNode<T> node2, Direction direction, ref T value)
//        {
//            ILinkedGridNode<T> nodeAbove = node2;

//            while (nodeAbove[direction] != null)
//            {
//                nodeAbove = nodeAbove[direction];
//            }

//            nodeAbove[direction] = new LinkedGridNode<T>(ref value);

//            var oppisteDir = direction == Direction.Up ? Direction.Down : Direction.Up;
//            nodeAbove[direction][oppisteDir] = nodeAbove;
//            CopyConectionsToChild(node2, nodeAbove[direction]);

//            verticalNodesCount++;

//            return nodeAbove[direction];
//        }

//        private void CopyConectionsToChild(ILinkedGridNode<T> parent, ILinkedGridNode<T> child)
//        {
//            foreach (var connection in parent.Connections)
//            {
//                if (connection.Key != Direction.Up && connection.Key != Direction.Down)
//                {
//                    child.Connections.Add(connection.Key, connection.Value);
//                }
//            }
//        }

//        public void AddFirst(LinkedGridNode<T> node2)
//        {
//            node2[Direction.Next] = First;
//            First[Direction.Previous] = node2;
//            first = node2;

//            count++;
//            TryAddHeader();
//            CreateSideConnections();
//        }
//        public LinkedGridNode<T> AddFirst(T value)
//        {
//            throw new NotImplementedException();
//        }
//        public void AddLast(LinkedGridNode<T> node2)
//        {
//            node2[Direction.Previous] = Last;
//            Last[Direction.Next] = node2;
//            last = node2;

//            count++;
//            TryAddHeader();
//            CreateSideConnections();
//        }
//        public LinkedGridNode<T> AddLast(T value)
//        {
//            throw new NotImplementedException();
//        }

//        public void Clear()
//        {
//            first = null;
//            last = null;
//        }

//        public bool Contains(T item)
//        {
//            ILinkedGridNode<T> node2 = First;
//            while (node2 != Last)
//            {
//                if (node2.Node.Position == item.Position)
//                {
//                    return true;
//                }

//                node2 = node2[Direction.Next];
//            }

//            return false;
//        }

//        public void CopyTo(Array array, int index)
//        {
//            throw new NotImplementedException();
//        }

//        public void CopyTo(T[] array, int arrayIndex)
//        {
//            throw new NotImplementedException();
//        }

//        public IEnumerator GetEnumerator()
//        {
//            var enumerator = new LinkedGridEnumerator<LinkedGridNode<T>, T>();
//            enumerator.Initilize(first);

//            return enumerator;
//        }

//        public bool Remove(T item)
//        {
//            ILinkedGridNode<T> node2 = First;
//            while (node2 != Last)
//            {
//                if (node2.Node.Position == item.Position)
//                {
//                    if (node2[Direction.Previous] != null)
//                    {
//                        node2[Direction.Previous][Direction.Next] = node2[Direction.Next];
//                        node2[Direction.Next][Direction.Previous] = node2[Direction.Previous];
//                        return true;
//                    }
//                }

//                node2 = node2[Direction.Next];
//            }

//            return false;
//        }

//        public IPathNode[][][] ToArray()
//        {
//            if (Count <= 0) return null;

//            IPathNode[][][] connections = new IPathNode[count][][];
//            int columnSize = 0;
//            int x = -1;
//            var node2 = First;

//            try
//            {
//                for (int y = 0; node2 != null; y++)
//                {
//                    var curNode = node2;

//                    if (x + 1 < Headers.Count && curNode == Headers[x + 1])
//                    {
//                        try
//                        {
//                            if (Headers.Count > x + 2)
//                                columnSize = checked((int)(Headers[x + 2].Node.ID - Headers[x + 1].Node.ID));
//                            else
//                                columnSize = checked((int)(last.ID - Headers[x + 1].Node.ID + 1));
//                        }
//                        catch (OverflowException)
//                        {
//                            throw new OverflowException();
//                        }

//                        connections[x + 1] = new IPathNode[columnSize][];
//                        ++x;
//                        y = 0;
//                    }

//                    List<IPathNode> zNodes = null;
//                    for (int z = 0; curNode != null; z++)
//                    {
//                        zNodes ??= new();
//                        zNodes.Add(node2.GetNodeCopy());
//                        curNode = curNode[Direction.Down];
//                    }
//                    connections[x][y] = zNodes.ToArray();

//                    node2 = node2[Direction.Next];
//                }
//            }
//            catch (ArgumentOutOfRangeException)
//            {

//                throw;
//            }

//            return connections;
//        }

//        IEnumerator<T> IEnumerable<T>.GetEnumerator()
//        {
//            //return (IEnumerator<T>)new LinkedGridEnumerator<LinkedGridNode<T>, T>(First);
//            throw new NotImplementedException();
//        }
//    }

//    public class LinkedGridEnumerator<T, U> : ILinkedEnumeratorIn<T, U>, ILinkedGridEnumerator<T, U>, IEnumerator<T> where T : LinkedGridNode<U> where U : IPathNode
//    {
//        T current;

//        public LinkedGridEnumerator()
//        {
//        }

//        public T Current => current;

//        object IEnumerator.Current => current;

//        public void Initilize(T first)
//        {
//            current = (T)new LinkedGridNode<U>();
//            current[Direction.Next] = first;
//        }

//        public void Dispose()
//        {
//            current = null;
//        }

//        public bool MoveNext()
//        {
//            if (current[Direction.Next] == null) return false;

//            current = (T)current[Direction.Next];
//            return true;
//        }

//        public void Reset()
//        {
//            throw new NotImplementedException();
//        }
//    }
//    public enum Direction
//    {
//        None,
//        Previous,
//        Next,
//        Up,
//        Down,
//        Right,
//        Left,
//    }

//    public class LinkedGridNode<T> : ILinkedGridNodeIN<T>, ILinkedGridNode<T> where T : IPathNode
//    {
//        Dictionary<Direction, ILinkedGridNode<T>> connections = new();
//        T node2;
//        int gapSize;
//        int columnIdx;

//        public T Node { get => node2; }
//        public int RowIdx { get => gapSize; }
//        public int ColumnIdx { get => columnIdx; }
//        public uint ID { get => node2.ID; }
//        public ILinkedGridNode<T> GetDir(Direction d)
//        {
//            return connections.ContainsKey(d) ? connections[d] : null;
//        }

//        public void SetDir(Direction d, ILinkedGridNode<T> value)
//        {
//            if (connections.ContainsKey(d))
//            {
//                connections[d] = value;
//            }
//            else
//            {
//                connections.Add(d, value);
//            }

//            if (d == Direction.Next || d == Direction.Left || d == Direction.Right)
//            {
//                if (connections.ContainsKey(Direction.Up))
//                {
//                    ((ILinkedGridNodeIN<T>)connections[Direction.Up].GetDir(d)).SetDir(Direction.Up, value);
//                }
//            }
//        }

//        public LinkedGridNode()
//        {

//        }

//        public LinkedGridNode(ref T node2, int gapSize = 0, int columnIdx = 0)
//        {
//            this.node2 = node2;
//            this.gapSize = gapSize;
//            this.columnIdx = columnIdx;
//        }

//        public LinkedGridNode(ILinkedGridNode<T> previous, ref T node2)
//        {
//            connections = new()
//            {
//                { Direction.Previous, previous }
//            };
//            this.node2 = node2;
//        }

//        public Dictionary<Direction, ILinkedGridNode<T>> Connections { get => connections; }

//        public T GetNodeCopy()
//        {
//            return node2;
//        }
//    }
//}
#endregion
