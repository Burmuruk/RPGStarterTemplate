using Burmuruk.AI;
using Burmuruk.RPGStarterTemplate.Saving;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static Burmuruk.RPGStarterTemplate.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.RPGStarterTemplate.Editor.Controls
{
    public class NavGenerator : SubWindow
    {
        #region Variables
        [Header("General Settings")]
        [Space]
        [SerializeField] bool detectSize = false;
        [SerializeField] GameObject p1;
        [SerializeField] GameObject p2;
        [SerializeField] int layer;
        [SerializeField] bool is3d;

        [Header("Octree Settings")]
        [SerializeField] float nodeMinSize = 5;
        [SerializeField] int maxDepth = 16;

        private Octree octree;
        private Graph waypoints;
        NodesList nodesList;
        #endregion

        public VisualElement StatusContainer { get; private set; }
        public Label LblSceneName { get; private set; }
        public Label LblOctreeState{ get; private set; }
        public Label LblMeshState { get; private set; }
        public Toggle TglDetectSize { get; private set; }
        public VisualElement PointsContainer { get; private set; }
        public Toggle TglShowArea { get; private set; }
        public ObjectField P1 { get; private set; }
        public ObjectField P2 { get; private set; }
        public Toggle TglOctree { get; private set; }
        public FloatField NodeMinSize { get; private set; }
        public IntegerField MaxDepth { get; private set; }
        public Toggle TglMesh { get; private set; }
        public VisualElement OctreeControls { get; private set; }
        public VisualElement MeshControls { get; private set; }
        public Button BtnGenerate { get; private set; }
        public Button BtnDelete{ get; private set; }

        public void Initialize(VisualElement container, VisualElement buttonsContainer)
        {
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/com.burmuruk.rpg-starter-template/Tool/UIToolkit/SystemEditor/Tabs/NavGeneration.uxml");
            container.Add(visualTree.Instantiate());
            Initialize(container);

            StatusContainer = container.Q<VisualElement>("StatusContainer");
            LblSceneName = container.Q<VisualElement>("LblSceneName").Q<Label>();
            LblMeshState = container.Q<VisualElement>("LblMeshState").Q<Label>();
            LblOctreeState = container.Q<VisualElement>("LblOctreeState").Q<Label>();
            TglDetectSize = container.Q<Toggle>("TglDetectSize");
            TglShowArea = container.Q<Toggle>("TglShowArea");
            PointsContainer = container.Q<VisualElement>("PointsContainer");
            TglOctree = container.Q<Toggle>("TglIs3D");
            MaxDepth = container.Q<IntegerField>("IFMaxDepth");
            NodeMinSize = container.Q<FloatField>("FFMinSize");
            TglMesh = container.Q<Toggle>("TglMesh");
            OctreeControls = container.Q<VisualElement>("OctreeControls");
            MeshControls = container.Q<VisualElement>("MeshControls");
            BtnGenerate = buttonsContainer.parent.Q<Button>("BtnGenerate");
            BtnDelete = buttonsContainer.parent.Q<Button>("BtnDelete");

            P1 = new ObjectField("Corner 1");
            P1.objectType = typeof(GameObject);
            PointsContainer.Add(P1);
            P2 = new ObjectField("Corner 2");
            P2.objectType = typeof(GameObject);
            PointsContainer.Add(P2);

            TglDetectSize.RegisterValueChangedCallback(EnableSizeDetection);
            TglOctree.RegisterValueChangedCallback(EnableOctreeControls);
            TglMesh.RegisterValueChangedCallback(EnableMeshControls);
            TglShowArea.RegisterValueChangedCallback(EnableShowArea);
            BtnDelete.clicked += RemoveData;
            BtnGenerate.clicked += GenerateNavigation;

            Clear();
            LoadInfo();
        }

        private void EnableShowArea(ChangeEvent<bool> evt)
        {
            if (evt.newValue)
            {
                Highlight(P1, false);
                Highlight(P2, false);
                bool isValid = true;

                if (P1.value == null)
                {
                    isValid = false;
                    Highlight(P1, true, BorderColour.Error);
                }
                if (P2.value == null)
                {
                    isValid = false;
                    Highlight(P2, true, BorderColour.Error);
                }

                if (!isValid)
                {
                    TglShowArea.SetValueWithoutNotify(false);
                    return;
                }
            }

            if (nodesList != null)
                nodesList.showMeshZone = evt.newValue;
        }

        private void LoadInfo()
        {
            LblSceneName.text = SceneManager.GetActiveScene().name;

            bool found = CheckNavFileStatus();

            LblMeshState.text = found ? "Saved" : "None";
            var colour = found ? BorderColour.Success : BorderColour.LightBorder;
            Highlight(StatusContainer, true, colour);

            BtnDelete.SetEnabled(found);
        }

        bool CheckNavFileStatus()
        {
            int sceneIdx = SceneManager.GetActiveScene().buildIndex;

            JsonWriter jsonWriter = new();
            var path = jsonWriter.GetPathFromSaveFile("Saving" + sceneIdx);

            return File.Exists(path);
        }

        private void GenerateNavigation()
        {
            if (!VerifyData())
            {
                Notify("Invalid data", BorderColour.Error);
                return;
            }
        }

        private void RemoveData()
        {
            throw new NotImplementedException();
        }

        private void EnableMeshControls(ChangeEvent<bool> evt)
        {
            if (evt.newValue)
            {
                nodesList ??= ScriptableObject.CreateInstance<NodesList>();

                MeshControls.Add(new InspectorElement(nodesList));
                EnableContainer(MeshControls, true);
                var asset = AssetDatabase.LoadAllAssetsAtPath("Assets/com.burmuruk.rpg-starter-template/Tool/Prefabs/Editor/Navigation/Node.prefab");

                if (asset == null)
                    Debug.Log("Object not found");
                else if (asset.Length > 0)
                {
                    if ((asset[0] as GameObject) != null)
                        nodesList.debugNode = asset[0] as GameObject;
                }
            }
            else
                MeshControls.Clear();

            BtnGenerate.SetEnabled(evt.newValue || BtnGenerate.enabledSelf);
        }

        private void EnableSizeDetection(ChangeEvent<bool> evt)
        {
            detectSize = evt.newValue;

            EnableContainer(PointsContainer, !evt.newValue);
        }

        private void EnableOctreeControls(ChangeEvent<bool> evt)
        {
            if (evt.newValue)
            {
                bool isValid = true;

                if (NodeMinSize.value <= 0)
                {
                    isValid = false;
                    Highlight(NodeMinSize, true, BorderColour.Error);
                }
                if (MaxDepth.value <= 0)
                {
                    isValid = false;
                    Highlight(MaxDepth, true, BorderColour.Error);
                }

                if (!isValid)
                {
                    TglOctree.SetValueWithoutNotify(false);
                    return;
                }
            }
            
            EnableContainer(MaxDepth.parent, evt.newValue);
            EnableContainer(NodeMinSize.parent, evt.newValue);
            BtnGenerate.SetEnabled(evt.newValue || BtnDelete.enabledSelf);
        }

        public override bool VerifyData()
        {
            return true;
        }

        public override ModificationTypes Check_Changes()
        {
            return ModificationTypes.None;
        }

        public override void Clear()
        {
            EnableContainer(OctreeControls, false);
            EnableContainer(MeshControls, false);
            EnableContainer(PointsContainer, false);
            TglOctree.value = false;
            TglMesh.value = false;
            TglDetectSize.value = true;
            EnableContainer(MaxDepth.parent, false);
            EnableContainer(NodeMinSize.parent, false);
            TglDetectSize.SetValueWithoutNotify(true);
            BtnDelete.SetEnabled(false);
            BtnGenerate.SetEnabled(false);

            NodeMinSize.value = 5;
            MaxDepth.value = 16;
        }

        public override void Load_Changes()
        {
            throw new NotImplementedException();
        }

        #region Unity methods
        void Start()
        {
            CreateOctree();
            CreateNavMesh();
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

        #region Creation
        private void CreateOctree()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            waypoints = new Graph();
            GetInitialSize(out Vector3 center, out Vector3 size, out float maxSize);

            Debug.Log("start time: " + sw.ElapsedMilliseconds + " ms");
            octree = new Octree(GetObjectsInArea(center, size), nodeMinSize, waypoints, maxDepth);
            Debug.Log("total time: " + sw.ElapsedMilliseconds + " ms");
            sw.Stop();
        }

        private void CreateNavMesh()
        {
            nodesList ??= ScriptableObject.CreateInstance<NodesList>();
            //nodesList.SetAvailableAreaDetector();
            nodesList.Calculate_PathMesh();
            nodesList.CalculateNodesConnections();
            nodesList.SaveList();
        }
        #endregion

        #region Initial size detection
        private void GetInitialSize(out Vector3 center, out Vector3 size, out float MaxSize)
        {
            if (detectSize)
            {
                DectecInitialSize(out center, out size);
            }
            else
                size = GetAreaSize(out center);

            float max1 = MathF.Max(size.x, size.y);
            MaxSize = MathF.Max(max1, size.z);
        }

        private void DectecInitialSize(out Vector3 center, out Vector3 size)
        {
            center = Vector3.zero;
            size = Vector3.one * 5;
            Bounds bounds = new Bounds(center, Vector3.one);
            var hits = Physics.BoxCastAll(center, size, Vector3.up, Quaternion.identity, .1f, 1 << layer);
            RaycastHit[] newHits = hits;

            do
            {
                hits = newHits;

                if (hits.Length > 0)
                {
                    foreach (var item in hits)
                    {
                        bounds.Encapsulate(item.collider.bounds);
                    }
                }
                else
                {
                    Debug.LogError("No objects found in the specified area. Please ensure there are objects to generate the octree.");
                    size = Vector3.zero;
                    return;
                }

                size = bounds.size;
                newHits = Physics.BoxCastAll(center, size, Vector3.up, Quaternion.identity, .1f, 1 << layer);
            } while (newHits.Length > hits.Length);

            Debug.DrawRay(bounds.center, Vector3.up * bounds.extents.y, Color.yellow, 5f);
            Debug.DrawRay(bounds.center, Vector3.down * bounds.extents.y, Color.yellow, 5f);
            Debug.DrawRay(bounds.center, Vector3.right * bounds.extents.x, Color.yellow, 5f);
            Debug.DrawRay(bounds.center, Vector3.left * bounds.extents.x, Color.yellow, 5f);
            Debug.Log("Initial size detected: " + size + " at center: " + bounds.center);
        }

        private GameObject[] GetObjectsInArea(in Vector3 center, in Vector3 size)
        {
            var newSize = size * .5f;
            var hits = Physics.BoxCastAll(center, newSize, Vector3.up, Quaternion.identity, .1f, 1 << layer);

            return (from objects in hits select objects.collider.gameObject).ToArray();
        }

        private Vector3 GetAreaSize(out Vector3 center)
        {
            float x, y, z;
            float px, py, pz;

            (x, px) = GetSizeAndCenter(p1.transform.position.x, p2.transform.position.x);
            (y, py) = GetSizeAndCenter(p1.transform.position.y, p2.transform.position.y);
            (z, pz) = GetSizeAndCenter(p1.transform.position.z, p2.transform.position.z);

            center = new Vector3(px, py, pz);
            return new Vector3(x, y, z);

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
        #endregion
    }
}