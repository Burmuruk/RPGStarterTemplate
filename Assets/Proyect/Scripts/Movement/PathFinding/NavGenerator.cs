using Burmuruk.AI;
using Burmuruk.Collections;
using Burmuruk.WorldG.Patrol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Burmuruk.Tesis.Movement.PathFindig
{
    public class NavGenerator : MonoBehaviour
    {
        #region Variables
        [Header("Nodes Settings")]
        [Space]
        [SerializeField] bool is3d;
        [SerializeField] float nodDistance = 3;
        [SerializeField] float pRadious = .5f;
        [SerializeField] float maxAngle = 45;
        [SerializeField] bool showChanges = false;

        [Header("Octree Settings")]
        [SerializeField] private int nodeMinSize = 5;
        GameObject[] worldObjects;
        private Octree octree;
        private Graph waypoints;

        [Header("Mesh Settings")]
        [Space]
        [SerializeField] GameObject debugNode;
        [SerializeField] GameObject x1;
        [SerializeField] GameObject x2;
        [SerializeField] bool canCreateMesh = false;
        [SerializeField] bool addDynamicObjs = true;
        [SerializeField] bool showMeshZone = false;
        [SerializeField] bool phisicNodes = false;
        [SerializeField] int layer;

        [Header("Status"), Space()]
        [SerializeField, Space()] uint nodeCount = 0;
        public pState meshState = pState.None;
        public pState connectionsState = pState.None;
        public pState octreeState = pState.None;
        public pState memorySaved = pState.None;

        private LinkedGrid<IPathNode> nodes;
        IPathNode[][][] connections;
        #endregion

        #region Unity methods
        void Start()
        {
            CreateOctree();
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                octree.rootNode.Draw();
            }
        }
        #endregion

        private void CreateOctree()
        {
            waypoints = new Graph();
            DetectInitialSize(out Vector3 center, out Vector3 size, out float maxSize);

            octree = new Octree(GetObjectsInArea(center, size), nodDistance, waypoints);
        }

        private void DetectInitialSize(out Vector3 center, out Vector3 size, out float MaxSize)
        {
            size = GetAreaSize(out center);

            float max1 = MathF.Max(size.x, size.y);
            MaxSize = MathF.Max(max1, size.z);
        }

        private GameObject[] GetObjectsInArea(in Vector3 center, in Vector3 size)
        {
            var newSize = size * .5f;
            var hits = Physics.BoxCastAll(center, newSize, Vector3.up, Quaternion.identity, .1f, 1 << layer);

            return (from objects in hits select objects.collider.gameObject).ToArray();
        }

        private Vector3 GetAreaSize(out Vector3 center)
        {
            float x,y, z;
            float px, py, pz;

            (x, px) = GetSizeAndCenter(x1.transform.position.x, x2.transform.position.x);
            (y, py) = GetSizeAndCenter(x1.transform.position.y, x2.transform.position.y);
            (z, pz) = GetSizeAndCenter(x1.transform.position.z, x2.transform.position.z);

            center = new Vector3(px, py, pz);
            return new Vector3(x, y ,z);

            (float distance, float position) GetSizeAndCenter(float x1, float x2)
            {
                return (x1 < 0, x2 < 0) switch
                {
                    (true, true) => GetCenterFromSize(x1, MathF.Abs(x1 - x2)),
                    (false, true) => GetCenterFromSize(x2, x1 + MathF.Abs(x2)),
                    (true, false) => GetCenterFromSize(x1, MathF.Abs(x1) + x2),
                    (false, false) => GetCenterFromSize(x1, MathF.Abs(x1 - x2)),
                };
            }

            (float distance, float center) GetCenterFromSize(in float start, in float distance) =>
                (distance, start + distance * .5f);
        }
    }
}