using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public List<Edge> edgeList = new List<Edge>();
    public Node path = null;
    public OctreeNode octreeNode;
    public float f, g, h;
    public Node cameFrom;

    public Node(OctreeNode octreeNode)
    {
        this.octreeNode = octreeNode;
        path = null;
    }

    public OctreeNode GetNode()
    {
        return octreeNode;
    }
}
