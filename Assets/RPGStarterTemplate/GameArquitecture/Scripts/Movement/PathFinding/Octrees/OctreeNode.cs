using Assets.Octrees;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public struct OctreeObject
{
    public readonly GameObject gameObject;
    public readonly Bounds bounds;

    public OctreeObject(GameObject obj)
    {
        gameObject = obj;
        bounds = obj.GetComponent<Collider>().bounds;
    }
}

public class OctreeNode
{
    public int id;
    public Bounds nodeBounds;
    Bounds[] childBounds;
    public OctreeNode[] children = null;
    float minSize;
    public List<OctreeObject> containedObjects = new List<OctreeObject>();
    public OctreeNode parent;

    public OctreeNode(Bounds nodeBounds, float minNodeSize, OctreeNode parent)
    {
        this.nodeBounds = nodeBounds;
        minSize = minNodeSize;
        id = Utils.idNumber++;
        this.parent = parent;

        float quarter = nodeBounds.extents.y / 2;
        float childLength = nodeBounds.extents.y;

        Vector3 childSize = Vector3.one * childLength;
        childBounds = new Bounds[8];
        childBounds[0] = new Bounds(nodeBounds.center + new Vector3(-quarter, quarter, -quarter), childSize);
        childBounds[1] = new Bounds(nodeBounds.center + new Vector3(quarter, quarter, -quarter), childSize);
        childBounds[2] = new Bounds(nodeBounds.center + new Vector3(-quarter, quarter, quarter), childSize);
        childBounds[3] = new Bounds(nodeBounds.center + new Vector3(quarter, quarter, quarter), childSize);
        childBounds[4] = new Bounds(nodeBounds.center + new Vector3(-quarter, -quarter, -quarter), childSize);
        childBounds[5] = new Bounds(nodeBounds.center + new Vector3(quarter, -quarter, -quarter), childSize);
        childBounds[6] = new Bounds(nodeBounds.center + new Vector3(-quarter, -quarter, quarter), childSize);
        childBounds[7] = new Bounds(nodeBounds.center + new Vector3(quarter, -quarter, quarter), childSize);
        this.parent = parent;
    }

    public void AddObject(GameObject go)
    {
        DivideAndAdd(go);
    }

    public void DivideAndAdd(GameObject go)
    {
        OctreeObject octObj = new OctreeObject(go);
        if (nodeBounds.size.y <= minSize)
        {
            containedObjects.Add(octObj);
            return;
        }
        
        if (children == null)
            children = new OctreeNode[8];
        bool dividing = false;

        for (int i = 0; i < 8; i++)
        {
            if (children[i] == null)
                children[i] = new OctreeNode(childBounds[i], minSize, this);

            //if (childBounds[i].Contains(octObj.bounds.min) && childBounds[i].Contains(octObj.bounds.max))
            if (childBounds[i].Intersects(octObj.bounds))
            {
                dividing = true;
                children[i].DivideAndAdd(go);
            }
        }

        if (!dividing)
        {
            containedObjects.Add(octObj);
            children = null;
        }
    }

    public void Draw()
    {
        //Gizmos.color = Color.green;
        //Gizmos.DrawWireCube(nodeBounds.center, nodeBounds.size);

        Gizmos.color = Color.red;
        foreach (var obj in containedObjects)
        {
            Gizmos.DrawWireCube(obj.bounds.center, obj.bounds.size);
        }

        if (children != null)
        {
            for (int i = 0; i < 8; i++)
            {
                if (children[i] != null)
                {
                    children[i].Draw();
                }
            }
        }
        else if (containedObjects.Count != 0)
        {
            Gizmos.color = new Color(0, 0, 1, .25f);
            Gizmos.DrawCube(nodeBounds.center, nodeBounds.size);
        }
    }
}