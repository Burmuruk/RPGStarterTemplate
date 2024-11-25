using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Octree
{
    public OctreeNode rootNode;
    public List<OctreeNode> emptyLeaves = new();
    public Graph navigationGraph;

    public Octree(GameObject[] worldObjects, float minNodeSize, Graph navGraph)
    {
        Bounds bounds = new Bounds();
        navigationGraph = navGraph;

        foreach (var go in worldObjects)
        {
            bounds.Encapsulate(go.GetComponent<Collider>().bounds);
        }

        float maxSize = Mathf.Max(new float[] { bounds.size.x, bounds.size.y, bounds.size.z});
        Vector3 sizeVector = Vector3.one * maxSize * 1f;
        bounds.SetMinMax(bounds.center - sizeVector, bounds.center  + sizeVector);

        rootNode = new OctreeNode(bounds, minNodeSize, null);
        AddObjects(worldObjects);
        GetEmptyLeaves(rootNode);
        ConnectLeafNodeNeighbours();
        //ProcessExtraConnections();
    }

    public void AddObjects(GameObject[] worldObjects)
    {
        foreach (var go in worldObjects)
        {
            rootNode.AddObject(go);
        }
    }

    public int FindBindingNode(OctreeNode node, Vector3 position)
    {
        int found = -1;
        
        if (node == null) return -1;

        if (node.children == null)
        {
            if (node.nodeBounds.Contains(position) && node.containedObjects.Count == 0)
            {
                return node.id; 
            }
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                found = FindBindingNode(node.children[i], position);

                if (found != -1) break;
            }
        }

        return found;
    }

    public void GetEmptyLeaves(OctreeNode node)
    {
        if (node == null) return;

        if (node.children == null)
        {
            if (node.containedObjects.Count == 0)
            {
                emptyLeaves.Add(node);
                navigationGraph.AddNode(node);
            }
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                GetEmptyLeaves(node.children[i]);

                //for (int s = 0; s < 8; s++)
                //{
                //    if (s != i)
                //    {
                //        navigationGraph.AddEdge(node.children[i], node.children[s]);
                //    }
                //}
            }
        }
    }

    void ProcessExtraConnections()
    {
        Dictionary<int, int> subGraphConnections = new Dictionary<int, int>();

        foreach (OctreeNode i in emptyLeaves)
        {
            foreach (OctreeNode j in emptyLeaves)
            {
                if (i.id != j.id && i.parent.id != j.parent.id)
                {
                    RaycastHit hitInfo;
                    Vector3 direction = j.nodeBounds.center - i.nodeBounds.center;
                    float accuracy = 1;
                    if (!Physics.SphereCast(i.nodeBounds.center, accuracy, direction, out hitInfo))
                    {
                        if (subGraphConnections.TryAdd(i.parent.id, j.parent.id))
                            navigationGraph.AddEdge(i, j);
                    }
                }
            }
        }
    }

    void ConnectLeafNodeNeighbours()
    {
        List<Vector3> rays = new List<Vector3>()
        {
            new Vector3(1, 0, 0),
            new Vector3(-1, 0, 0),
            new Vector3(1, 1, 0),
            new Vector3(1, -1, 0),
            new Vector3(1, 0, 1),
            new Vector3(1, 0, -1),
        };

        for (int i = 0; i < emptyLeaves.Count; i++)
        {
            List<OctreeNode> neighbours = new List<OctreeNode>();

            for (int j = 0; j < emptyLeaves.Count; j++)
            {
                if (i != j)
                {
                    for (int k = 0; k < 6; k++)
                    {
                        Ray ray = new Ray(emptyLeaves[i].nodeBounds.center, rays[k]);
                        float maxLenght = emptyLeaves[i].nodeBounds.size.y / 2 + 0.01f;

                        if (emptyLeaves[j].nodeBounds.IntersectRay(ray, out float hitLength))
                        {
                            if (hitLength < maxLenght)
                                neighbours.Add(emptyLeaves[j]);
                        }
                    }
                }
            }

            foreach (var octreeNode in neighbours)
            {
                navigationGraph.AddEdge(emptyLeaves[i], octreeNode);
            }
        }
    }
}
