using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.RPGStarterTemplate.Dialogue.Editor
{
    public class DialogueGraphEditor : EditorWindow
    {
        private VisualElement canvas;
        private Vector2 dragStart;

        [MenuItem("RPGTemplate/Dialogue Editor (UI Toolkit)")]
        public static void ShowEditor()
        {
            DialogueGraphEditor wnd = GetWindow<DialogueGraphEditor>();
            wnd.titleContent = new GUIContent("Dialogue Graph");
        }

        public void CreateGUI()
        {
            // Cargar USS
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/com.burmuruk.rpg-starter-template/Tool/UIToolkit/Styles/Node.uss");
            rootVisualElement.styleSheets.Add(styleSheet);


            // Canvas Scroll
            var scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            scrollView.style.flexGrow = 1;
            rootVisualElement.Add(scrollView);

            // Canvas interno para nodos
            canvas = new VisualElement();
            canvas.style.width = 4000;
            canvas.style.height = 4000;
            canvas.style.position = Position.Relative;
            scrollView.Add(canvas);
            scrollView.scrollOffset = new Vector2(2000, 2000);

            // Botón de prueba
            var button = new Button(() =>
            {
                scrollView.scrollOffset = new Vector2(2000, 2000);
                scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
                scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
                AddNode(new Vector2(2000, 2000));
            })
            { text = "Añadir Nodo" };
            rootVisualElement.Add(button);
        }

        private void AddNode(Vector2 position)
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/com.burmuruk.rpg-starter-template/Tool/UIToolkit/DialogueEditor/Elements/Node.uxml");
            var node = visualTree.CloneTree();
            node.name = "Node";
            node.style.position = Position.Absolute;
            node.style.left = position.x;
            node.style.top = position.y;
            node.AddToClassList("dialogue-node");

            MakeDraggable(node);

            canvas.Add(node);
        }

        private void MakeDraggable(VisualElement node)
        {
            Vector2 offset = Vector2.zero;

            node.RegisterCallback<PointerDownEvent>(evt =>
            {
                offset = evt.position;
                evt.StopPropagation();
            });

            node.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (evt.pressedButtons == 1)
                {
                    Vector2 delta = (Vector2)evt.position - offset;
                    offset = evt.position;

                    var left = node.resolvedStyle.left + delta.x;
                    var top = node.resolvedStyle.top + delta.y;

                    node.style.left = left;
                    node.style.top = top;
                }
            });
        }
            }


    /*
    public class DialogueGraphEditor : EditorWindow
{
    private VisualElement canvas;
    private Vector2 canvasSize = new Vector2(5000, 5000);
    private Vector2 initialScroll = new Vector2(2500, 2500);

    [MenuItem("Window/Dialogue Graph Editor")]
    public static void ShowWindow()
    {
        DialogueGraphEditor wnd = GetWindow<DialogueGraphEditor>();
        wnd.titleContent = new GUIContent("Dialogue Graph Editor");
    }

    public void CreateGUI()
    {
        canvas = new VisualElement();
        canvas.style.width = canvasSize.x;
        canvas.style.height = canvasSize.y;
        canvas.style.position = Position.Relative;

        var scroll = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
        scroll.style.flexGrow = 1.0f;
        scroll.Add(canvas);

        rootVisualElement.Add(scroll);
        scroll.scrollOffset = initialScroll;

        CreateNode(initialScroll);
    }

    private void CreateNode(Vector2 position, DialogueNodeView connectFrom = null)
    {
        var node = new DialogueNodeView();
        node.style.left = position.x;
        node.style.top = position.y;
        canvas.Add(node);

        if (connectFrom != null)
        {
            var edge = new EdgeView(connectFrom, node);
            canvas.Add(edge);
        }

        node.OnAddRequested = () =>
        {
            Vector2 newPosition = position + new Vector2(200, 150);
            CreateNode(newPosition, node);
        };
    }
}

public class DialogueNodeView : VisualElement
{
    public System.Action OnAddRequested;

    private bool isDragging;
    private Vector2 dragOffset;

    public DialogueNodeView()
    {
        style.position = Position.Absolute;
        style.width = 200;
        style.height = 100;
        style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        style.borderBottomLeftRadius = 4;
        style.borderBottomRightRadius = 4;
        style.borderTopLeftRadius = 4;
        style.borderTopRightRadius = 4;

        var button = new Button(() => OnAddRequested?.Invoke()) { text = "+" };
        Add(button);

        RegisterCallback<PointerDownEvent>(OnPointerDown);
        RegisterCallback<PointerMoveEvent>(OnPointerMove);
        RegisterCallback<PointerUpEvent>(OnPointerUp);
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        isDragging = true;
        dragOffset = evt.position - this.worldBound.position;
        CaptureMouse();
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!isDragging) return;

        Vector2 newPos = evt.position - dragOffset;
        style.left = newPos.x;
        style.top = newPos.y;
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        isDragging = false;
        ReleaseMouse();
    }
}

public class EdgeView : VisualElement
{
    private DialogueNodeView fromNode;
    private DialogueNodeView toNode;

    public EdgeView(DialogueNodeView from, DialogueNodeView to)
    {
        fromNode = from;
        toNode = to;

        style.position = Position.Absolute;
        style.backgroundColor = Color.red;
        style.width = 2;
        style.height = 2;

        generateVisualContent += ctx =>
        {
            var p1 = fromNode.worldBound.center;
            var p2 = toNode.worldBound.center;
            Handles.DrawBezier(p1, p2, p1 + Vector2.right * 50, p2 + Vector2.left * 50, Color.white, null, 2);
        };
    }
}








    */
}
