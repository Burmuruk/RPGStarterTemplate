using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor {
    public partial class TabCharacterEditor : BaseLevelEditor {
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
        VisualElement componentsContainer;
        DropdownField ddfAddComponent;
        VisualElement statsContainer;
        EnumField efCharacterType;
        List<ElementComponent> components = new();
        CharacterData characterData;

        const string btnSettingAcceptName = "btnSettingAccept";
        const string btnSettingCancelName = "btnSettingCancel";

        struct ElementComponent {
            public VisualElement element;
            public int index;
            Label _label;
            Toggle _toggle;
            Button _button;

            public Label Label {
                get {
                    if (_label == null) {
                        _label = element.Q<Label>();
                    }

                    return _label;
                }
            }

            public Toggle Toggle {
                get {
                    if (_toggle == null) {
                        _toggle = element.Q<Toggle>();
                    }

                    return _toggle;
                }
            }

            public Button Button {
                get {
                    if (_button == null) {
                        _button = element.Q<Button>();
                    }

                    return _button;
                }
            }

            public ElementComponent(VisualElement element) {
                this.element = element;
                index = 0;
                _label = null;
                _toggle = null;
                _button = null;
            }
        }

        public class StatsVisualizer : ScriptableObject {
            [SerializeField] BasicStats stats;
        }

        private void Create_CharacterTab() {
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

        private void OnCancel_BtnSetting() {
            switch (currentSettingTag) {
                case ElementType.Character:
                    Discard_CharacterChanges();
                    break;
                default: break;
            }
        }

        private void OnAccept_BtnAccept() {
            switch (currentSettingTag) {
                case ElementType.Character:
                    SaveChanges_Character();
                    break;
                default: break;
            }
        }

        private void Populate_EFCharacterType() {
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

        private bool Check_HasCharacterComponent(string value) {
            for (int i = 0; i < components.Count; i++) {
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
            Debug.Log("creating new component");
            components.Add(component);
            return component;
        }

        private void Disable_CharacterComponents() =>
            components.ForEach(c => EnableContainer(c.element, false));

        private void RemoveComponent(int idx) 
        {
            EnableContainer(components[idx].element, false);
        }

        private void SaveChanges_Character() 
        {
            if (string.IsNullOrEmpty(txtNameCreation.value)) return;

            int idx = 0;
            foreach (var creationType in charactersLists.elements.Keys) 
            {
                foreach (var elementName in charactersLists.elements[creationType]) 
                {
                    if (elementName == txtNameCreation.value) 
                    {
                        Notify("There's already an element with the same name.", BorderColour.Warning);
                        Debug.Log("Save canceled");
                        break;
                    }

                    ++idx;
                }
            }

            if (!charactersLists.creations.ContainsKey(ElementType.Character))
                charactersLists.creations.Add(ElementType.Character, new());

            charactersLists.creations[ElementType.Character].TryAdd(txtNameCreation.value, new());

            charactersLists.creations[ElementType.Character][txtNameCreation.value] = characterData;
            charactersLists.elements[ElementType.Character].Add(txtNameCreation.value);

            //EditorUtility.SetDirty(charactersLists);
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();

            SearchAllElements();
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

            Disable_CharacterComponents();
            Debug.Log(data.components.Count);
            data.components.ForEach(c => Add_CharacterComponent(c.ToString()));

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
