using Burmuruk.Tesis.Stats;
using Burmuruk.Tesis.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        const string btnGoBackSettings = "btnGoBack";
        const string infoExtreSettingName = "infoContainer";

        VisualElement infoExtraSetting;
        VisualElement componentsContainer;
        DropdownField ddfAddComponent;
        VisualElement statsContainer;
        EnumModifier emCharacterType;
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
            public ComponentType type;
            Button _btnEditComponent;
            Toggle _toggle;
            Button _btnRemove;

            public Button BtnEditComponent
            {
                get
                {
                    if (_btnEditComponent == null)
                    {
                        _btnEditComponent = element.Q<Button>("btnEditComponent");
                    }

                    return _btnEditComponent;
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

            public Button BtnRemove
            {
                get
                {
                    if (_btnRemove == null)
                    {
                        _btnRemove = element.Q<Button>("btnRemove");
                    }

                    return _btnRemove;
                }
            }

            public ElementComponent(VisualElement element)
            {
                this.element = element;
                index = 0;
                _btnEditComponent = null;
                _toggle = null;
                _btnRemove = null;
                type = ComponentType.None;
            }
        }

        struct EnumModifier
        {
            public const string ContainerName = "EnumModifier";
            public VisualElement Container { get; private set; }
            public Button BtnAddValue { get; private set; }
            public EnumField EnumField { get; private set; }
            public TextField TxtNewValue { get; private set; }
            public VisualElement EnumContainer { get; private set; }
            public VisualElement NewValueContainer { get; private set; }

            public EnumModifier(VisualElement container)
            {
                this.Container = container;
                BtnAddValue = container.Q<Button>();
                EnumField = container.Q<EnumField>();
                TxtNewValue = container.Q<TextField>();
                EnumContainer = container.Q<VisualElement>("EnumLine");
                NewValueContainer = container.Q<VisualElement>("NewElementLine");
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

            emCharacterType = new EnumModifier(infoContainers[infoCharacterName].Q<VisualElement>(EnumModifier.ContainerName));
            emCharacterType.BtnAddValue.clicked += () => Toggle_EnumAddingState(emCharacterType);
            emCharacterType.EnumField.RegisterValueChangedCallback(SetCharacterType);

            btnSettingCancel = infoSetup.Q<Button>(btnSettingCancelName);
            btnSettingAccept = infoSetup.Q<Button>(btnSettingAcceptName);
            btnSettingCancel.clicked += OnCancel_BtnSetting;
            btnSettingAccept.clicked += OnAccept_BtnAccept;
            emCharacterType.TxtNewValue.RegisterCallback<KeyUpEvent>(OnKeyUp_TxtCharacterType);
            EnableContainer(emCharacterType.NewValueContainer, false);

            ddfAddComponent.RegisterValueChangedCallback(AddComponent);
            characterData = new CharacterData();
            Populate_AddComponents();
            Populate_EFCharacterType();

            var instance = ScriptableObject.CreateInstance<StatsVisualizer>();
            statsContainer.Add(new InspectorElement(instance));
        }

        private void OnKeyUp_TxtCharacterType(KeyUpEvent evt)
        {
            if (evt.keyCode == KeyCode.Return)
            {
                if (!IsNameWrittenCorrectly(emCharacterType.TxtNewValue.value))
                    return;
                
                if (!Add_EnumValue("CharacterType")) return;
                
                EnableContainer(emCharacterType.NewValueContainer, true);
                Toggle_EnumAddingState(emCharacterType);
                emCharacterType.EnumField.SetValueWithoutNotify(CharacterType.None);
            }
        }

        private bool Add_EnumValue(string EnumName)
        {
            EnumEditor enumEditor = new();
            string error = "";
            //enumEditor.Modify(EnumName, new string[] { emCharacterType.TxtNewValue.value }, "Path", out string error);

            if (!string.IsNullOrEmpty(error))
            {
                Notify(error, BorderColour.Error);
                return false;
            }

            Notify("Value added", BorderColour.Approved);
            return true;
        }

        private void Toggle_EnumAddingState(EnumModifier modifier)
        {
            bool shouldAddValue = modifier.BtnAddValue.text == "+";
            modifier.BtnAddValue.text = shouldAddValue ? "-" : "+";
            modifier.EnumField.SetEnabled(!shouldAddValue);
            EnableContainer(modifier.NewValueContainer, shouldAddValue);
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

                case ElementType.Item:

                    break;

                case ElementType.Weapon:

                    break;

                case ElementType.Armor:

                    break;

                case ElementType.Buff:

                    break;

                case ElementType.Consumable:

                    break;

                default: break;
            }
        }

        private void Populate_EFCharacterType()
        {
            emCharacterType.EnumField.Init(CharacterType.None);
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

            Add_CharacterComponent(evt.newValue, Enum.Parse<ComponentType>(evt.newValue));

            ddfAddComponent.SetValueWithoutNotify("None");
        }

        private bool Check_HasCharacterComponent(string value)
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (!components[i].element.ClassListContains("Disable") && components[i].BtnEditComponent.text.Contains(value))
                    return true;
            }

            return false;
        }

        private void Add_CharacterComponent(string value, ComponentType type)
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

            if (componentIdx == -1)
                CreateNewComponent(value, out componentIdx);

            var componentData = components[componentIdx];
            componentData.type = type;
            components[componentIdx] = componentData;
            components[componentIdx].BtnEditComponent.text = value;

            characterData.components ??= new();
            characterData.components.Add(type, null);

            Setup_ComponentButton(type, componentIdx);
            if (type == ComponentType.Equipment && !(from comp in components where comp.type == ComponentType.Inventory select comp).Any())
            {
                Add_CharacterComponent(ComponentType.Inventory.ToString(), ComponentType.Inventory);
            }
        }

        private void Setup_ComponentButton(ComponentType type, int componentIdx)
        {
            switch (type)
            {
                case ComponentType.Inventory:
                    goto case ComponentType.Health;

                case ComponentType.Equipment:
                    goto case ComponentType.Health;

                case ComponentType.Health:
                    SetClickableButtonColour(componentIdx);
                    break;

                default:
                    if (components[componentIdx].BtnEditComponent.ClassListContains("ClickableBtn"))
                        components[componentIdx].BtnEditComponent.RemoveFromClassList("ClickableBtn");
                    components[componentIdx].BtnEditComponent.style.backgroundColor = new Color(0.1647059f, 0.1647059f, 0.1647059f);
                    break;
            }
        }

        private void SetClickableButtonColour(int componentIdx)
        {
            components[componentIdx].BtnEditComponent.AddToClassList("ClickableBtn");
            components[componentIdx].BtnEditComponent.style.backgroundColor = new Color(0.4627451f, 0.4627451f, 4627451f);
        }

        private ElementComponent CreateNewComponent(string value, out int idx)
        {
            VisualTreeAsset element = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Proyect/Game/UIToolkit/ElementComponent.uxml");
            var component = new ElementComponent(element.Instantiate());
            
            idx = components.Count;
            int newIdx = idx;
            component.index = idx;

            component.BtnEditComponent.clicked += () => OpenComponentSettings(newIdx);
            component.BtnRemove.clicked += () => RemoveComponent(newIdx);

            components.Add(component);
            componentsContainer.Add(components[idx].element);
            return component;
        }

        private void OpenComponentSettings(int componentIdx)
        {
            var type = components[componentIdx].type;
            
            string tabName = type switch
            {
                ComponentType.Equipment => infoEquipmentSettingsName,
                ComponentType.Health => infoHealthSettingsName,
                ComponentType.Inventory => infoInventorySettingsName,
                _ => null
            };

            if (tabName == null) return;

            switch (type) 
            { 
                case ComponentType.Equipment:
                    curElementList = mclEquipmentElements;
                    Load_InventoryItemsInEquipment();
                    break;
                case ComponentType.Inventory:
                    curElementList = mclInventoryElements;
                    break;

                default: break;
            }

            ChangeTab(tabName);
        }

        private void Disable_CharacterComponents() =>
            components.ForEach(c => EnableContainer(c.element, false));

        private void RemoveComponent(int idx)
        {
            if (components[idx].type == ComponentType.Inventory &&
                (from comp in components where comp.type == ComponentType.Equipment select comp).Any())
            {
                Notify("Equipment requires an Inventory component to store the items", BorderColour.Error);
                return;
            }

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
                        Notify("There's already an element with the same name.", BorderColour.Error);
                        Debug.Log("Save canceled");
                        return true;
                    }

                    ++idx;
                }
            }

            return false;
        }

        private void Discard_CharacterChanges()
        {
            txtNameCreation.value = "";
            CFCreationColor.value = Color.black;
            components.ForEach(c => EnableContainer(c.element, false));
            ddfAddComponent.SetValueWithoutNotify("None");
            emCharacterType.EnumField.SetValueWithoutNotify(CharacterType.None);

            var instance = ScriptableObject.CreateInstance<StatsVisualizer>();
            statsContainer.Clear();
            statsContainer.Add(new InspectorElement(instance));
            characterData = new();
        }
    }
}
