using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static Burmuruk.RPGStarterTemplate.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.RPGStarterTemplate.Editor.Dialogue
{
    public enum NodeType
    {
        None,
        Dialogue,
        Mission,
        SubMission
    }

    public class DialogueEditor : EditorWindow
    {
        private DialogueGraphView graphView;
        DialogueGraphData controller;
        private DialogueGraphController _settingsTab;
        VisualElement configTab;
        BaseNode selectedNode;

        [MenuItem("RPGTemplate/Dialogue Editor (UI Toolkit)")]
        public static void ShowEditorWindow()
        {
            GetWindow(typeof(DialogueEditor), false, "Dialogue Editor");
        }

        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var dialogue = EditorUtility.InstanceIDToObject(instanceID) as RPGStarterTemplate.Dialogue.Dialogue;

            if (dialogue != null)
            {
                ShowEditorWindow();
                return true;
            }

            return false;
        }

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
            CreateGraphView();
        }

        private void CreateGraphView()
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/com.burmuruk.rpg-starter-template/Tool/UIToolkit/Styles/BasicSS.uss");
            rootVisualElement.styleSheets.Add(styleSheet);
            graphView = new DialogueGraphView
            {
                name = "Dialogue Graph"
            };
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);

            controller = CreateInstance<DialogueGraphData>();
            _settingsTab = CreateInstance<DialogueGraphController>();
            _settingsTab.Initialize(controller);
            configTab = _settingsTab.SettingsContainer;

            rootVisualElement.Add(_settingsTab.Container);

            configTab.style.visibility = Visibility.Hidden;
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            graphView.OnNodeCreated += controller.AddNode;
            graphView.OnNodeDeleted += RemoveNode;

            graphView.OnNodeCreated += (node) =>
            {
                node.OnSelected += (n) =>
                {
                    DisplayNodeOptions(n, true);
                    _settingsTab.SetTargetNode(n);
                };
                node.OnDeselected += (n) =>
                {
                    DisplayNodeOptions(n, false);
                    _settingsTab.SetTargetNode(n);
                };
                node.OnPinned += (element, pinned) =>
                {
                    if (pinned)
                    {
                        _settingsTab.AddPin(element);
                    }
                    else
                    {
                        _settingsTab.RemovePin(element);
                    }
                };
            };
        }

        private void RemoveNode(GraphViewNode graphNode)
        {
            foreach (var node in controller.nodes)
            {
                if (graphNode == node.GraphViewNode)
                {
                    controller.RemoveNode(node);
                    return;
                }
            }

        }

        private void DisplayNodeOptions(BaseNode node, bool shouldDisplay)
        {
            if (graphView.selection.Count != 1 || graphView.selection[0] is not GraphViewNode)
            {
                configTab.style.visibility = Visibility.Hidden;
                return;
            }

            switch (node)
            {
                case DialogueNode:
                    configTab.style.visibility = shouldDisplay ? Visibility.Visible : Visibility.Hidden;
                    break;
                default:
                    break;
            }
        }

        private void OnSelectionChanged()
        {
            //var newDialogue = Selection.activeObject as RPGStarterTemplate.Dialogue.Dialogue;

            //if (newDialogue != null)
            //{
            //    selectedDialogue = newDialogue;
            //    Repaint();
            //}
        }
    }

    public class DialogueGraphView : GraphView
    {
        private NodeSearchProvider _searchProvider;

        public event Action<BaseNode> OnNodeCreated;
        public event Action<GraphViewNode> OnNodeDeleted;

        public DialogueGraphView()
        {
            GridBackground grid = new();
            Insert(0, grid);
            grid.StretchToParentSize();

            this.SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.contentContainer.style.width = 5000;
            this.contentContainer.style.height = 5000;

            _searchProvider = ScriptableObject.CreateInstance<NodeSearchProvider>();
            _searchProvider.Init(this);

            //schedule.Execute(ResetPositionAndScale).ExecuteLater(1000);
            // Centrar vista en el medio
            //ScheduleExecute(() => ClearAndCenterView());
            this.schedule.Execute(() =>
            {
                var node = ScriptableObject.CreateInstance<DialogueNode>();
                AddElement(CreateNode(new Vector2(100, 100), NodeType.Dialogue));
            }).ExecuteLater(500);
        }

        void ResetPositionAndScale()
        {
            contentViewContainer.transform.position = -new Vector3(2500, 2500, 0);
            contentViewContainer.transform.scale = Vector3.one;
        }

        private void ScheduleExecute(System.Action action)
        {
            schedule.Execute(() =>
            {
                action.Invoke();
            }).ExecuteLater(100);
        }

        private void ClearAndCenterView()
        {
            Vector2 center = new Vector2(contentContainer.layout.width / 2, contentContainer.layout.height / 2);
            contentViewContainer.transform.position = -center;
            contentViewContainer.transform.scale = Vector3.one;
        }

        public GraphViewNode CreateNode(Vector2 position, NodeType type)
        {
            var node = InstanciateNode(type);
            node.Initilize(this, position);
            AddElement(node.GraphViewNode);

            OnNodeCreated?.Invoke(node);
            return node.GraphViewNode;
        }

        private BaseNode InstanciateNode(NodeType type) =>
            type switch
            {
                NodeType.Dialogue => ScriptableObject.CreateInstance<DialogueNode>(),
                NodeType.Mission => ScriptableObject.CreateInstance<MissionNode>(),
                _ => ScriptableObject.CreateInstance<BaseNode>()
            };

        public void CreateConnectedNode(GraphViewNode fromNode)
        {
            var fromPort = fromNode.output;
            var toNode = CreateNode(fromNode.GetPosition().position + new Vector2(250, 0), NodeType.Dialogue);
            var toPort = toNode.input;

            var edge = fromPort.ConnectTo(toPort);
            AddElement(edge);
        }

        public void Connect(Port from, Port to)
        {
            var edge = from.ConnectTo(to);
            AddElement(edge);
        }

        // Abre el buscador para crear nodo y conectar desde 'fromPort'
        public void OpenCreateNodeSearch(Vector2 dropPosition, Port fromPort)
        {
            // dropPosition ya viene en coords del graph (contentViewContainer) en versiones recientes.
            // Si ves desalineación, convierte: dropPosition = contentViewContainer.WorldToLocal(dropPosition);

            _searchProvider.SetupInvocation(fromPort, dropPosition);

            // Convierte a pantalla para SearchWindow
            var screenPos = GUIUtility.GUIToScreenPoint(Event.current != null ? Event.current.mousePosition : Vector2.zero);
            SearchWindow.Open(new SearchWindowContext(screenPos), _searchProvider);
        }

        public override EventPropagation DeleteSelection()
        {
            foreach (var node in selection)
            {
                if (node is GraphViewNode graphNode)
                {
                    OnNodeDeleted?.Invoke(graphNode);
                }
            }

            return base.DeleteSelection();
        }
    }
}
