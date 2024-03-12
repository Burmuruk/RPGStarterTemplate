using Burmuruk.Tesis.Movement.PathFindig;
using UnityEngine;

namespace Burmuruk.AI
{
    public class PathWriter : MonoBehaviour
    {
        [SerializeField] public Path path;
        public NodesList nodesList;

        public void SaveConnections()
        {
            nodesList.FreeMemory();
            path.SaveList(nodesList);
        }
    }
}
