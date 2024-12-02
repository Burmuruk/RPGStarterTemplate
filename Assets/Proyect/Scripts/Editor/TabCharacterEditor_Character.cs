using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Burmuruk.Tesis.Stats;
using System.ComponentModel;
using System;
using static UnityEditor.Rendering.FilterWindow;
using UnityEditor.UIElements;

namespace Burmuruk.Tesis.Editor
{
	public partial class TabCharacterEditor : BaseLevelEditor
	{
        const string txtCreationName = "";
        const string creationColorName = "";
        TextField txtNameCreation;
        Button btnSettingAccept;
        Button btnSettingCancel;

        const string ComponentsContainerName = "componentsConatiner";
        const string settingColourContainer = "ColourContainer";
        const string ddfAddComponentName = "ddfAddComponent";
        const string statsContainerName = "infoStats";
        const string efCharacterTypeName = "ddfCharaterType";
        VisualElement componentsContainer;
        DropdownField ddfAddComponent;
        VisualElement statsContainer;
        EnumField efCharacterType;
        List<ElementComponent> components = new();
        CharacterData characterData;

        const string btnSettingAcceptName = "btnSettingAccept";
        const string btnSettingCancelName = "btnSettingCancel";

        struct ElementComponent
        {
            public VisualElement element;
            public int index;
            Label _label;
            Toggle _toggle;
            Button _button;

            public Label Label
            {
                get
                {
                    if (_label == null)
                    {
                        _label = element.Q<Label>();
                    }

                    return _label;
                }
            }

            public Toggle Toggle
            {
                get
                {
                    if (_toggle == null)
                    {
                        _toggle = element.Q<Toggle>();
                    }

                    return _toggle;
                }
            }

            public Button Button
            {
                get
                {
                    if (_button == null)
                    {
                        _button = element.Q<Button>();
                    }

                    return _button;
                }
            }

            public ElementComponent(VisualElement element)
            {
                this.element = element;
                index = 0;
                _label = null;
                _toggle = null;
                _button = null;
            }
        }

        public class StatsVisualizer : ScriptableObject
        {
            [SerializeField] BasicStats stats;
        }

        private void Create_CharacterTab()
        {
            componentsContainer = infoContainers[infoCharacterName].Q<VisualElement>(ComponentsContainerName);
            ddfAddComponent = infoContainers[infoCharacterName].Q<DropdownField>(ddfAddComponentName);
            statsContainer = infoContainers[infoCharacterName].Q<VisualElement>(statsContainerName);
            efCharacterType = infoContainers[infoCharacterName].Q<EnumField>(efCharacterTypeName);

            ddfAddComponent.RegisterValueChangedCallback(AddComponent);
            //ddfAddComponent.set
            ddfAddComponent.SetValueWithoutNotify("None");
            efCharacterType.RegisterValueChangedCallback(SetCharacterType);
            characterData = new CharacterData();

            var instance = ScriptableObject.CreateInstance<StatsVisualizer>();
            statsContainer.Add(new InspectorElement(instance));
        }

        private void SetCharacterType(ChangeEvent<Enum> evt)
        {
            characterData.characterType = (CharacterType)evt.newValue;
        }

        private void AddComponent(ChangeEvent<string> evt)
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].element.ClassListContains("Disable"))
                    break;

                var lastData = components[i];
                lastData.Label.text = evt.newValue;
                lastData.Toggle.value = false;
                components[i] = lastData;

                EnableContainer(components[i].element, true);
                return;
            }

            VisualTreeAsset element = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Proyect/Game/UIToolkit/ElementComponent.uxml");
            var component = new ElementComponent(element.Instantiate());
            component.Label.text = evt.newValue.ToString();
            int idx = components.Count;
            component.index = idx;
            component.Button.clicked += () => RemoveComponent(idx); 

            components.Add(component);
            componentsContainer.Add(component.element);

            ddfAddComponent.SetValueWithoutNotify("None");
        }

        private void RemoveComponent(int idx)
        {
            EnableContainer(components[idx].element, false);
        }

        private void SaveChanges()
        {

        }

        private void DiscardChanges()
        {

        }

        struct CharacterData
        {
            public string characterName;
            public Color color;
            public List<ComponentType> components;
            public BasicStats stats;
            public CharacterType characterType;
        }
    } 
}
