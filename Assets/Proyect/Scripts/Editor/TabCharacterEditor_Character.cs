using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
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
        EnumModifierUI emCharacterType;
        List<ElementComponent> creations = new();
        CharacterData characterData;
        StatsVisualizer basicStats = null;

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

        public class StatsVisualizer : ScriptableObject
        {
            [SerializeField] public BasicStats stats;
        }

        private void Create_BaseSettingsTab()
        {
            btnSettingCancel = infoSetup.Q<Button>(btnSettingCancelName);
            btnSettingAccept = infoSetup.Q<Button>(btnSettingAcceptName);
            btnSettingCancel.clicked += OnCancel_BtnSetting;
            btnSettingAccept.clicked += OnAccept_BtnAccept;
        }

        private void Create_CharacterTab()
        {
            componentsContainer = infoContainers[infoCharacterName].Q<VisualElement>(ComponentsContainerName);
            ddfAddComponent = infoContainers[infoCharacterName].Q<DropdownField>(ddfAddComponentName);
            statsContainer = infoContainers[infoCharacterName].Q<VisualElement>(statsContainerName);

            emCharacterType = new EnumModifierUI(infoContainers[infoCharacterName].Q<VisualElement>(EnumModifierUI.ContainerName), Notify, CharacterType.None);
            emCharacterType.EnumField.RegisterValueChangedCallback(SetCharacterType);

            ddfAddComponent.RegisterValueChangedCallback(AddComponent);
            characterData = new CharacterData();
            Populate_AddComponents();
            Populate_EFCharacterType();

            var instance = ScriptableObject.CreateInstance<StatsVisualizer>();
            statsContainer.Add(new InspectorElement(instance));
            basicStats = instance;

            VisualElement adderUI = infoContainers[infoCharacterName].Q<VisualElement>("VariblesAdder");
            VariablesAdderUI adder = new(adderUI, statsContainer);
        }

        private void OnCancel_BtnSetting()
        {
            switch (currentSettingTag.type)
            {
                case ElementType.Character:
                    Discard_CharacterChanges();
                    break;
                case ElementType.Weapon:
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
                    if (!Create_Settings())
                        return;
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

        private bool Edit_Settings()
        {
            switch (currentSettingTag.type)
            {
                case ElementType.Character:
                    if (SaveChanges_Character(editingElement.name, txtNameCreation.value))
                    {
                        Notify("Changes applied.", BorderColour.Approved);
                    }
                    else return false;
                    break;

                case ElementType.Buff:
                    SaveElement(ElementType.Buff, "", curBuffData.buff, txtNameCreation.text);
                    break;

                case ElementType.Item:
                case ElementType.Weapon:
                case ElementType.Armour:
                case ElementType.Consumable:

                    string creationName = currentSettingTag.type switch
                    {
                        ElementType.Item => curItemData.Name,
                        ElementType.Weapon => curWeaponData.Name,
                        ElementType.Armour => curArmorData.Name,
                        ElementType.Consumable => curConsumableData.Name,
                        _ => null
                    };

                    SaveElement(ElementType.Armour, creationName, curArmorData, txtNameCreation.text);
                    break;

                default: break;
            }

            return true;
        }

        private bool Create_Settings()
        {
            switch (currentSettingTag.type)
            {
                case ElementType.Character:
                    if (SaveChanges_Character(txtNameCreation.value))
                    {
                        Notify("Character created.", BorderColour.Approved);
                    }
                    else return false;
                    break;

                case ElementType.Item:
                case ElementType.Weapon:
                case ElementType.Armour:
                case ElementType.Consumable:
                    object creationData = currentSettingTag.type switch
                    {
                        ElementType.Item => curItemData,
                        ElementType.Weapon => curWeaponData,
                        ElementType.Armour => curArmorData,
                        ElementType.Consumable => curConsumableData,
                        _ => null
                    };

                    SaveElement(ElementType.Armour, txtNameCreation.text, creationData);
                    break;

                case ElementType.Buff:
                    ref BuffData buffData = ref curBuffData.buff;
                    SaveElement(ElementType.Buff, txtNameCreation.text, buffData);
                    break;

                default: break;
            }

            return true;
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
            for (int i = 0; i < curElementList.Components.Count; i++)
            {
                if (!curElementList.Components[i].element.ClassListContains("Disable") && curElementList.Components[i].NameButton.text.Contains(value))
                    return true;
            }

            return false;
        }

        private void Add_CharacterComponent(string value, Enum type)
        {
            int componentIdx = -1;

            for (int i = 0; i < curElementList.Components.Count; i++)
            {
                if (curElementList.Components[i].element.ClassListContains("Disable"))
                {
                    componentIdx = i;
                    EnableContainer(curElementList.Components[i].element, true);
                    break;
                }
            }

            if (componentIdx == -1)
                CreateNewComponent(value, out componentIdx);

            var componentData = curElementList.Components[componentIdx];
            componentData.Type = type;
            curElementList.Components[componentIdx] = componentData;
            curElementList.Components[componentIdx].NameButton.text = value;


            if (type is ComponentType)
            {
                var compType = (ComponentType)type;
                Setup_ComponentButton(compType, componentIdx);

                if (compType == ComponentType.Equipment && !(from comp in curElementList.Components where ((ComponentType)comp.Type) == ComponentType.Inventory select comp).Any())
                {
                    Add_CharacterComponent(ComponentType.Inventory.ToString(), ComponentType.Inventory);
                }
            }
        }

        private void Setup_ComponentButton(Enum type, int componentIdx)
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
                    if (creations[componentIdx].BtnEditComponent.ClassListContains("ClickableBtn"))
                        creations[componentIdx].BtnEditComponent.RemoveFromClassList("ClickableBtn");
                    creations[componentIdx].BtnEditComponent.style.backgroundColor = new Color(0.1647059f, 0.1647059f, 0.1647059f);
                    break;
            }
        }

        private void SetClickableButtonColour(int componentIdx)
        {
            creations[componentIdx].BtnEditComponent.AddToClassList("ClickableBtn");
            creations[componentIdx].BtnEditComponent.style.backgroundColor = new Color(0.4627451f, 0.4627451f, 4627451f);
        }

        private ElementComponent CreateNewComponent(string value, out int idx)
        {
            VisualTreeAsset element = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Proyect/Game/UIToolkit/CharacterEditor/Elements/ElementComponent.uxml");
            var component = new ElementComponent(element.Instantiate());

            idx = creations.Count;
            int newIdx = idx;
            component.index = idx;

            component.BtnEditComponent.clicked += () => OpenComponentSettings(newIdx);
            component.BtnRemove.clicked += () => RemoveComponent(newIdx);

            creations.Add(component);
            componentsContainer.Add(creations[idx].element);
            return component;
        }

        private void OpenComponentSettings(int componentIdx)
        {
            var type = creations[componentIdx].type;

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
            creations.ForEach(c => EnableContainer(c.element, false));

        private void RemoveComponent(int idx)
        {
            if (creations[idx].type == ComponentType.Inventory &&
                (from comp in creations where comp.type == ComponentType.Equipment select comp).Any())
            {
                Notify("Equipment requires an Inventory component to store the items", BorderColour.Error);
                return;
            }

            EnableContainer(creations[idx].element, false);
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
            creations.ForEach(c => EnableContainer(c.element, false));
            ddfAddComponent.SetValueWithoutNotify("None");
            emCharacterType.EnumField.SetValueWithoutNotify(CharacterType.None);

            var instance = ScriptableObject.CreateInstance<StatsVisualizer>();
            statsContainer.Clear();
            statsContainer.Add(new InspectorElement(instance));
            basicStats = instance;
            characterData = new();
        }

        public static bool VerifyVariableName(string name) => regName.IsMatch(name);
    }
}
