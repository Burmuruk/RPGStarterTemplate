using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.RPGStarterTemplate.Editor.Dialogue
{
    public class DialogueNode : BaseNode
    {
        public TextField TFMessage { get; private set; }
        public string Value 
        {
            get => TFMessage.value;
            set => TFMessage.value = value;
        }

        public override void Initilize(DialogueGraphView graph, Vector2 startPosition)
        {
            base.Initilize(graph, startPosition);
            AddMessageField();
        }

        private void AddMessageField()
        {
            var content = new VisualElement
            {
                style = {
                    flexDirection = FlexDirection.Column,
                    flexGrow = 1 ,
                    marginTop = 6,
                    marginBottom = 6,
                }
            };
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            row.Add(new Label("Message"));
            TFMessage = new TextField(500, true, false, '*')
            { style = { flexGrow = 1, flexShrink = 1, maxWidth = 400, flexBasis = 100 } };
            row.Add(TFMessage);

            content.Add(row);
            GraphViewNode.extensionContainer.Add(content);
        }
    }
}
