using Burmuruk.RPGStarterTemplate.Dialogue;
using Burmuruk.RPGStarterTemplate.Saving;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static Burmuruk.RPGStarterTemplate.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.RPGStarterTemplate.Editor.Dialogue
{
    public class DialogueGraphController : ScriptableObject
    {
        private const string SETTINGS_TAB_NAME = "Settings";
        private const string PINS_TAB_NAME = "Pins";
        private const string OF_CHARACTER_NAME = "OFCharacter";
        private const string TXT_ID = "txtId";
        private const string TXT_NICKNAME_NAME = "txtNickName";
        private const string CF_NODE_COLOUR_NAME = "NodeColour";
        private List<string> _pins = new();
        private DialogueGraphData _controller;
        private BaseNode _selectedNode;

        public event Action OnChange;

        public VisualElement SettingsContainer { get; private set; }
        public VisualElement PinsContainer { get; private set; }
        public ObjectField OFCharacter { get; private set; }
        public TextField TxtId { get; private set; }
        public TextField TxtNickName { get; private set; }
        public ColorField CFNodeColour { get; private set; }

        public VisualElement Container { get; private set; }

        public void Initialize(DialogueGraphData controller)
        {
            _controller = controller;
            Container = new VisualElement()
            {
                style = 
                { 
                    position = Position.Absolute,
                    flexDirection = FlexDirection.Row,
                    width = new Length(100, LengthUnit.Percent)
                }
            };
            CreateSettingsTab();
            CreatePinsTab();
            Container.Add(SettingsContainer);
            Container.Add(PinsContainer);

            SettingsContainer.style.visibility = Visibility.Hidden;
            EnableContainer(PinsContainer, false);
        }

        private void CreateSettingsTab()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/com.burmuruk.rpg-starter-template/Tool/UIToolkit/DialogueEditor/Elements/Node.uxml");
            var instance = visualTree.Instantiate();
            instance.style.position = Position.Absolute;
            //instance.style.right = 4;
            //instance.style.top = 4;

            SettingsContainer = instance.Q<VisualElement>(SETTINGS_TAB_NAME);
            TxtId = SettingsContainer.Q<TextField>(TXT_ID);
            TxtNickName = SettingsContainer.Q<TextField>(TXT_NICKNAME_NAME);
            TxtId.RegisterValueChangedCallback(SetNodesIds);
            TxtNickName.RegisterValueChangedCallback(SetNodesNames);

            CFNodeColour = new ColorField();
            CFNodeColour.style.flexBasis = 20;
            SettingsContainer.Q<VisualElement>(CF_NODE_COLOUR_NAME).Add(CFNodeColour);
            CFNodeColour.RegisterValueChangedCallback(ChangeNodesColor);

            OFCharacter = new ObjectField();
            OFCharacter.style.flexBasis = 20;
            SettingsContainer.Q<VisualElement>(OF_CHARACTER_NAME).Add(OFCharacter);
            OFCharacter.objectType = typeof(RPGStarterTemplate.Dialogue.AIConversant);
            OFCharacter.RegisterValueChangedCallback(VerifyCharacterSelection);
        }

        private void SetNodesNames(ChangeEvent<string> evt)
        {
            if (_selectedNode == null) return;

            var nodes = _controller.nodes.Except(new BaseNode[] { _selectedNode });
            //var nodes = _controller.nodes;
            //nodes.Remove(_selectedNode);

            foreach (var node in nodes)
            {
                if (node.characterID == _selectedNode.characterID)
                {
                    node.Title = evt.newValue;
                }
            }

            _selectedNode.Title = evt.newValue;
        }

        private void SetNodesIds(ChangeEvent<string> evt)
        {
            if (_selectedNode == null) return;

            var nodes = _controller.nodes.Except(new BaseNode[] { _selectedNode });

            foreach (var node in nodes)
            {
                if (node.characterID == _selectedNode.characterID)
                {
                    node.characterID = TxtId.value;
                }
            }

            _selectedNode.characterID = evt.newValue;
        }

        private void ChangeNodesColor(ChangeEvent<Color> evt)
        {
            if (_selectedNode == null) return;

            var nodes = _controller.nodes.Except(new BaseNode[] { _selectedNode });

            foreach (var node in _controller.nodes)
            {
                if (node.characterID == _selectedNode.characterID)
                {
                    node.GraphViewNode.style.backgroundColor = evt.newValue;
                }
            }

            _selectedNode.GraphViewNode.style.backgroundColor = evt.newValue;
        }

        public void SetTargetNode(BaseNode node)
        {
            _selectedNode = node;
            TxtId.SetValueWithoutNotify(node.characterID);
            TxtNickName.SetValueWithoutNotify(node.Title);
            CFNodeColour.SetValueWithoutNotify(node.GraphViewNode.style.backgroundColor.value);
            OFCharacter.SetValueWithoutNotify(null);
        }

        private void VerifyCharacterSelection(ChangeEvent<UnityEngine.Object> evt)
        {
            if (evt.newValue is not RPGStarterTemplate.Dialogue.AIConversant conversant)
            {
                TxtId.value = null;
                TxtNickName.value = null;
                return;
            }

            if (conversant.gameObject.TryGetComponent<JsonSaveableEntity>(out var saveableEntity))
            {
                var id = saveableEntity.GetUniqueIdentifier();
                SetPreviousSettings(id);

                TxtId.value = id;
                TxtNickName.value = conversant.GetName();
            }
            else
            {
                OFCharacter.SetValueWithoutNotify(null);
                Highlight(OFCharacter, 500, BorderColour.Error);
            }
        }

        private void SetPreviousSettings(string newId)
        {
            var nodes = _controller.nodes.Except(new BaseNode[] { _selectedNode });

            foreach (var node in nodes)
            {
                if (node.characterID == newId)
                {
                    _selectedNode.GraphViewNode.style.backgroundColor = node.GraphViewNode.style.backgroundColor;
                    break;
                }
            }
        }

        private void CreatePinsTab()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/com.burmuruk.rpg-starter-template/Tool/UIToolkit/DialogueEditor/PinsTab.uxml");
            var instance = visualTree.Instantiate();
            instance.style.position = Position.Absolute;
            instance.style.right = 10;
            instance.style.top = 4;

            PinsContainer = instance.Q<VisualElement>(PINS_TAB_NAME);
        }

        #region SettingsTab

        #endregion

        #region Pins
        public void AddPin(BaseNode node)
        {
            if (_pins.Contains(node.Id))
                return;

            _pins.Add(node.Id);

            if (_pins.Count >= 1)
            {
                EnableContainer(PinsContainer, true);
            }

            OnChange?.Invoke();
        }

        public void RemovePin(BaseNode node)
        {
            _pins.Remove(node.Id);

            if (_pins.Count == 0)
            {
                EnableContainer(PinsContainer, false);
            }

            OnChange?.Invoke();
        }
        #endregion
    }
}
