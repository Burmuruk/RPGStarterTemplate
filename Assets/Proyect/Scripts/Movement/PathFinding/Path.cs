using Burmuruk.AI;
using Burmuruk.AI.PathFinding;
using Burmuruk.WorldG.Patrol;
using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Movement.PathFindig
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Path", order = 2)]
    public class Path : ScriptableObject, INodeListSaver
    {
        [SerializeField] bool loaded;
        INodeListSupplier m_nodeList;

        public bool Loaded { get => loaded; }

        public void SaveList(INodeListSupplier nodeList)
        {
            this.m_nodeList = nodeList;
            loaded = true;
        }

        public INodeListSupplier GetNodeList()
        {
            return m_nodeList;
        }
    }
}
