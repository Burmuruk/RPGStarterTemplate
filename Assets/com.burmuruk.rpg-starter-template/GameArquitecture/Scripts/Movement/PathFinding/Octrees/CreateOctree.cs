using System.Collections;
using UnityEngine;

public class CreateOctree : MonoBehaviour
{
    public GameObject[] worldObjects;
    public int nodeMinSize = 5;
    public Octree ot;
    public Graph waypoints;

    void Start()
    {
        waypoints = new Graph();
        ot = new Octree(worldObjects, nodeMinSize, waypoints);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            ot.rootNode.Draw();
        }
    }
}