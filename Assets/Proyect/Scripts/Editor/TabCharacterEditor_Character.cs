using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public partial class TabCharacterEditor : BaseLevelEditor
    {
        const string txtCreationName = "txtName";
        const string creationColorName = "cfSettingColour";
        TextField txtNameCreation;
        ColorField CFCreationColor;
        Button btnSettingAccept;
        Button btnSettingCancel;

        const string ComponentsContainerName = "componentsConatiner";
        const string settingColourContainer = "ColourContainer";
        const string ddfAddComponentName = "ddfAddComponent";
        const string statsContainerName = "infoStats";
        const string efCharacterTypeName = "ddfCharaterType";
        const string btnGoBackSettings = "btnGoBack";
        const string infoExtreSettingName = "infoContainer";
        VisualElement infoExtreSetting;
        VisualElement componentsContainer;
        DropdownField ddfAddComponent;
        VisualElement statsContainer;
        EnumField efCharacterType;
        List<ElementComponent> components = new();
        CharacterData characterData;

        const string btnSettingAcceptName = "btnSettingAccept";
        const string btnSettingCancelName = "btnSettingCancel";
        (ElementType type, string name, int elementIdx) editingElement = default;
        SettingsState settingsState;

        enum SettingsState
        {
            None,
            Creating,
            Editing,
        }

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
            btnSettingCancel = infoSetup.Q<Button>(btnSettingCancelName);
            btnSettingAccept = infoSetup.Q<Button>(btnSettingAcceptName);
            btnSettingCancel.clicked += OnCancel_BtnSetting;
            btnSettingAccept.clicked += OnAccept_BtnAccept;

            ddfAddComponent.RegisterValueChangedCallback(AddComponent);
            efCharacterType.RegisterValueChangedCallback(SetCharacterType);
            characterData = new CharacterData();
            Populate_AddComponents();
            Populate_EFCharacterType();

            var instance = ScriptableObject.CreateInstance<StatsVisualizer>();
            statsContainer.Add(new InspectorElement(instance));
        }

        private void OnCancel_BtnSetting()
        {
            switch (currentSettingTag.type)
            {
                case ElementType.Character:
                    Discard_CharacterChanges();
                    break;
                default: break;
            }
        }

        private void OnAccept_BtnAccept()
        {
            switch (settingsState)
            {
                case SettingsState.None:
                    break;

                case SettingsState.Creating:
                    Create_Settings();
                    break;

                case SettingsState.Editing:
                    Edit_Settings();
                    break;

                default:
                    break;
            }

            settingsState = SettingsState.None;
            EnableContainer(infoSetup, false);
            Discard_CharacterChanges();
            Highlight(btnsRight_Tag[currentSettingTag.idx].element, false);
            currentSettingTag = (ElementType.None, -1);
            editingElement = (ElementType.None, "", -1);
        }

        private void Edit_Settings()
        {
            switch (currentSettingTag.type)
            {
                case ElementType.Character:
                    if (SaveChanges_Character(editingElement.name, txtNameCreation.value))
                    {
                        Notify("Changes applied.", BorderColour.Approved);
                    }
                    break;

                default: break;
            }
        }

        private void Create_Settings()
        {
            switch (currentSettingTag.type)
            {
                case ElementType.Character:
                    if (SaveChanges_Character(txtNameCreation.value))
                    {
                        Notify("Character created.", BorderColour.Approved);
                    }
                    break;

                default: break;
            }
        }

        private void Populate_EFCharacterType()
        {
            efCharacterType.Init(CharacterType.None);
        }

        private void Populate_AddComponents()
        {
            ddfAddComponent.choices.Clear();

            foreach (var type in Enum.GetValues(typeof(ComponentType)))
            {
                ddfAddComponent.choices.Add(type.ToString());
            }

            ddfAddComponent.SetValueWithoutNotify("None");
        }

        private void SetCharacterType(ChangeEvent<Enum> evt)
        {
            characterData.characterType = (CharacterType)evt.newValue;
        }

        private void AddComponent(ChangeEvent<string> evt)
        {
            if (Check_HasCharacterComponent(evt.newValue)) return;

            Add_CharacterComponent(evt.newValue);

            ddfAddComponent.SetValueWithoutNotify("None");
        }

        private bool Check_HasCharacterComponent(string value)
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (!components[i].element.ClassListContains("Disable") && components[i].Label.text.Contains(value))
                    return true;
            }

            return false;
        }

        private void Add_CharacterComponent(string value)
        {
            int componentIdx = -1;

            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].element.ClassListContains("Disable"))
                {
                    componentIdx = i;
                    EnableContainer(components[i].element, true);
                    break;
                }
            }

            if (componentIdx == -1) CreateNewComponent(value, out componentIdx);

            componentsContainer.Add(components[componentIdx].element);

            int idx = 0;
            foreach (var type in Enum.GetNames(typeof(ComponentType)))
            {
                if (type == components[componentIdx].Label.text)
                {
                    characterData.components ??= new();
                    characterData.components.Add((ComponentType)idx);
                    break;
                }

                ++idx;
            }
        }

        private ElementComponent CreateNewComponent(string value, out int idx)
        {
            VisualTreeAsset element = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Proyect/Game/UIToolkit/ElementComponent.uxml");
            var component = new ElementComponent(element.Instantiate());
            component.Label.text = value;
            idx = components.Count;
            int newIdx = idx;
            component.index = idx;
            component.Button.clicked += () => RemoveComponent(newIdx);

            components.Add(component);
            return component;
        }

        private void Disable_CharacterComponents() =>
            components.ForEach(c => EnableContainer(c.element, false));

        private void RemoveComponent(int idx)
        {
            EnableContainer(components[idx].element, false);
        }

        private bool IsTheNameUsed(string name)
        {
            int idx = 0;
            foreach (var creationType in charactersLists.elements.Keys)
            {
                foreach (var elementName in charactersLists.elements[creationType])
                {
                    if (elementName == name)
                    {
                        Notify("There's already an element with the same name.", BorderColour.Warning);
                        Debug.Log("Save canceled");
                        return true;
                    }

                    ++idx;
                }
            }

            return false;
        }

        private bool SaveChanges_Character(string name, string newName = "")
        {
            if (string.IsNullOrEmpty(name)) return false;

            if (IsTheNameUsed(newName)) return false;

            if (string.IsNullOrEmpty(newName)) newName = name;
            
            charactersLists.creations.TryAdd(ElementType.Character, new());

            if (charactersLists.creations[ElementType.Character].ContainsKey(name))
            {
                charactersLists.creations[ElementType.Character].Remove(name);
                charactersLists.elements[ElementType.Character].Remove(name);
            }

            characterData.characterName = newName;
            charactersLists.creations[ElementType.Character].TryAdd(newName, characterData);
            charactersLists.elements[ElementType.Character].Add(newName);

            //EditorUtility.SetDirty(charactersLists);
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();

            SearchAllElements();

            return true;
        }

        private void LoadChanges_Character(string elementName)
        {
            if (!charactersLists.creations[ElementType.Character].ContainsKey(elementName))
            {
                Debug.Log("No existe el elemento deseado");
                return;
            }

            CharacterData data = (CharacterData)charactersLists.creations[ElementType.Character][elementName];

            txtNameCreation.value = data.characterName;
            CFCreationColor.value = data.color;
            efCharacterType.value = data.characterType;
            characterData = data;
            
            Disable_CharacterComponents();

            characterData.components = new();
            if (data.components != null)
                for (int i = 0; i < data.components.Count; i++)
                {
                    Add_CharacterComponent(data.components[i].ToString());
                }

            ddfAddComponent.SetValueWithoutNotify("None");
        }

        private void Discard_CharacterChanges()
        {
            txtNameCreation.value = "";
            CFCreationColor.value = Color.black;
            components.ForEach(c => EnableContainer(c.element, false));
            ddfAddComponent.SetValueWithoutNotify("None");
            efCharacterType.SetValueWithoutNotify(CharacterType.None);

            var instance = ScriptableObject.CreateInstance<StatsVisualizer>();
            statsContainer.Clear();
            statsContainer.Add(new InspectorElement(instance));
            characterData = new();
        }
    }
}
