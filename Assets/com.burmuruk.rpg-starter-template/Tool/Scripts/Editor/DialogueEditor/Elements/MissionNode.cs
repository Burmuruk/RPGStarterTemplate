using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.RPGStarterTemplate.Editor.Dialogue
{
    public class MissionNode : BaseNode
    {
        public TextField TFTitle { get; private set; }
        public TextField TFDescription { get; private set; }
        public TextField TFInstructions { get; private set; }

        public override void Initilize(DialogueGraphView graph, Vector2 startPosition)
        {
            base.Initilize(graph, startPosition);

            TFTitle = AddTextField("Title");
            TFDescription = AddTextField("Description");
            TFInstructions = AddTextField("Instructions");
        }

        private TextField AddTextField(string tag)
        {
            TextField textField = null;
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
            row.Add(new Label(tag));
            textField = new TextField(500, true, false, '*')
            { style = { flexGrow = 1, flexShrink = 1, maxWidth = 400, flexBasis = 100 } };
            row.Add(textField);

            content.Add(row);
            GraphViewNode.extensionContainer.Add(row);
            return textField;
        }
    }
}
