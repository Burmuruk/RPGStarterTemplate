using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.RPGStarterTemplate.Editor.Dialogue
{
    public class GraphViewNode : Node
    {
        public const string PIN_BUTTON_NAME = "pinButton";
        public Port input;
        public Port output;
        private Button collapseButton;
        private bool collapsed = false;
        private DialogueGraphView graph;

        public event Action OnSelect;
        public event Action OnDeselected;

        public GraphViewNode(DialogueGraphView graphView)
        {
            graph = graphView;
            title = "No character";
            input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            input.portName = "Input";
            output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            output.portName = "Output";

            inputContainer.Add(input);
            outputContainer.Add(output);

            extensionContainer.Add(new VisualElement());
            extensionContainer.style.marginTop = 6;
            extensionContainer.style.marginBottom = 6;

            // Colors
            var colorRow = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            colorRow.Add(MakeColorButton(new Color(0.01568628f, 0.572549f, 0.6235294f)));
            colorRow.Add(MakeColorButton(new Color(0.254902f, 0.3490196f, 0.7333333f)));
            colorRow.Add(MakeColorButton(new Color(0.9921569f, 0.6941177f, 0.3176471f)));
            colorRow.Add(MakePinButton());
            colorRow.style.alignItems = Align.Center;
            titleButtonContainer.Add(colorRow);

            var listener = new CreateNodeEdgeConnectorListener(graph);
            output.AddManipulator(new EdgeConnector<Edge>(listener));
            input.AddManipulator(new EdgeConnector<Edge>(listener));

            //// Collapse button
            //collapseButton = new Button(() => ToggleCollapse()) { text = collapsed ? "?" : "?" };
            //extensionContainer.Add(collapseButton);

            //// Add (+) button
            //var addButton = new Button(() =>
            //{
            //    (this.GetFirstAncestorOfType<DialogueGraphView>()).CreateConnectedNode(this);
            //})
            //{ text = "+" };
            //titleButtonContainer.Add(addButton);

            //// Delete context menu
            //this.RegisterCallback<ContextClickEvent>(evt =>
            //{
            //    GenericMenu menu = new GenericMenu();
            //    menu.AddItem(new GUIContent("Delete Node"), false, () =>
            //    {
            //        var graph = this.GetFirstAncestorOfType<DialogueGraphView>();
            //        foreach (var edge in graph.edges.ToList())
            //        {
            //            if (edge.input.node == this || edge.output.node == this)
            //                graph.RemoveElement(edge);
            //        }
            //        graph.RemoveElement(this);
            //    });
            //    menu.ShowAsContext();
            //});

            RefreshExpandedState();
            RefreshPorts();
        }

        protected override void ToggleCollapse()
        {
            collapsed = !collapsed;
            extensionContainer.style.display = collapsed ? DisplayStyle.None : DisplayStyle.Flex;
            //collapseButton.text = collapsed ? "?" : "?";
        }

        public override void OnSelected()
        {
            base.OnSelected();
            OnSelect?.Invoke();
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            OnDeselected?.Invoke();
        }

        private Button MakeColorButton(Color color)
        {
            var button = new Button();
            button.style.borderTopColor = color;
            button.style.borderBottomColor = color;
            button.style.borderLeftColor = color;
            button.style.borderRightColor = color;
            button.style.borderTopWidth = 2;
            button.style.borderBottomWidth = 2;
            button.style.borderLeftWidth = 2;
            button.style.borderRightWidth = 2;
            button.style.marginLeft = 1;
            button.style.marginRight = 1;
            button.style.width = 20;
            button.style.height = 20;
            return button;
        }

        private Button MakePinButton()
        {
            var button = new Button();
            button.name = PIN_BUTTON_NAME;
            Texture2D pinIcon = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/com.burmuruk.rpg-starter-template/Tool/Art/Editor/Pin.png", typeof(Texture2D));
            button.style.backgroundImage = new StyleBackground(pinIcon);
            button.style.unityBackgroundImageTintColor = new Color(0.5169811f, 0.5169811f, 0.5169811f);
            button.style.marginLeft = 1;
            button.style.marginRight = 4;
            button.style.width = 20;
            button.style.height = 20;
            return button;
        }
    }

    class CreateNodeEdgeConnectorListener : IEdgeConnectorListener
    {
        private readonly DialogueGraphView graph;

        public CreateNodeEdgeConnectorListener(DialogueGraphView graphView)
        {
            graph = graphView;
        }

        // Soltó el cable sobre otro puerto: solo confirma la arista
        public void OnDrop(GraphView graphView, Edge edge)
        {
            graphView.AddElement(edge);
        }

        // Soltó el cable EN VACÍO: pedimos crear un nodo
        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            Port fromPort = edge.output ?? edge.input;
            edge.RemoveFromHierarchy();

            graph.OpenCreateNodeSearch(position, fromPort);
        }
    }
}
