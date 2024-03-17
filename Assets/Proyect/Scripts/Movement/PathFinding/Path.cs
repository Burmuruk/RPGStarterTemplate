using Burmuruk.AI;
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
        [SerializeField] public INodeListSupplier m_nodeList;
        [SerializeField] uint nodes;

        public bool Loaded { get => loaded; }
        public INodeListSupplier NodeList { get => m_nodeList; }

        public void SaveList(INodeListSupplier node)
        {
            m_nodeList = node;
            //this.m_nodeList = nodeList;
            loaded = true;
        }

        public INodeListSupplier GetNodeList()
        {
            return m_nodeList;
        }
    }
}
