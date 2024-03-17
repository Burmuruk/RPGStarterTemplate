using Burmuruk.Tesis.Movement;
using Burmuruk.Tesis.Movement.PathFindig;
using Burmuruk.WorldG.Patrol;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] Path path;
    private bool initialized = false;

    private void Awake()
    {
        
    }

    void Start()
    {
        
    }

    void Update()
    {
        SetPaths();
    }

    private void SetPaths()
    {
        if (initialized) return;

        if (path.Loaded && path.m_nodeList != null)
        {
            //print("Valor encontrado");
            var movers = FindObjectsOfType<Movement>();

            foreach (var mover in movers)
            {
                mover.SetConnections(path.m_nodeList);
            }

            initialized = true;
        }
    }


    public INodeListSupplier GetNodeList()
    {
        if (path == null && !path.Loaded) return null;

        return path.GetNodeList();
    }
}
