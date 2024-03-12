using Burmuruk.Tesis.Control;
using Burmuruk.Tesis.Movement.PathFindig;
using Burmuruk.WorldG.Patrol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] Path path;

    private void Awake()
    {

    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }


    public INodeListSupplier GetNodeList()
    {
        if (path == null && !path.Loaded) return null;

        return path.GetNodeList();
    }
}
