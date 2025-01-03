using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using System.Configuration;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public partial class TabCharacterEditor : BaseLevelEditor
    {
        const string TXT_CREATION_NAME = "txtName";
        const string CREATION_COLOUR_NAME = "cfSettingColour";
        TextField txtNameCreation;
        ColorField CFCreationColor;
        Button btnSettingAccept;
        Button btnSettingCancel;

        const string COMPONENTS_CONTAINER_NAME = "componentsConatiner";
        const string SETTINGS_COLOUR_CONTAINER = "ColourContainer";
        const string DDF_ADD_COMPONENT_NAME = "ddfElement";
        const string STATS_CONTAINER_NAME = "infoStats";
        const string BTN_GO_BACK_SETTINGS = "btnGoBack";
        const string INFO_EXTRA_SETTINGS_NAME = "infoContainer";

        VisualElement infoExtraSetting;
        VisualElement componentsContainer;
        DropdownField ddfAddComponent;
        VisualElement statsContainer;
        EnumModifierUI emCharacterType;
        CharacterData characterData;
        StatsVisualizer basicStats = null;

        const string BTN_SETTINGS_ACCEPT_NAME = "btnSettingAccept";
        const string BTN_SETTINGS_CANCEL_NAME = "btnSettingCancel";
        (ElementType type, string name, int elementIdx) editingElement = default;
        SettingsState settingsState;

        enum SettingsState
        {
            None,
            Creating,
            Editing,
        }

        public class StatsVisualizer : ScriptableObject
        {
            [SerializeField] public BasicStats stats;
        }

        private void Create_BaseSettingsTab()
        {
            btnSettingCancel = infoSetup.Q<Button>(BTN_SETTINGS_CANCEL_NAME);
            btnSettingAccept = infoSetup.Q<Button>(BTN_SETTINGS_ACCEPT_NAME);
            btnSettingCancel.clicked += OnCancel_BtnSetting;
            btnSettingAccept.clicked += OnAccept_BtnAccept;
        }

        private void Create_CharacterTab()
        {
            var componentsContainer = infoContainers[INFO_CHARACTER_NAME].Q<VisualElement>("infoComponents");
            characterComponents = new ComponentsListUI<ElementComponent>(componentsContainer, Notify);
            characterComponents.bindElementBtn += OpenComponentSettings;
            characterComponents.DDFElement.RegisterValueChangedCallback((e) => characterComponents.AddElement(e.newValue));

            statsContainer = infoContainers[INFO_CHARACTER_NAME].Q<VisualElement>(STATS_CONTAINER_NAME);

            emCharacterType = new EnumModifierUI(infoContainers[INFO_CHARACTER_NAME].Q<VisualElement>(EnumModifierUI.ContainerName), Notify, CharacterType.None);
            emCharacterType.EnumField.RegisterValueChangedCallback(SetCharacterType);

            characterData = new CharacterData();
            Populate_AddComponents();
            Populate_EFCharacterType();

            var instance = ScriptableObject.CreateInstance<StatsVisualizer>();
            statsContainer.Add(new InspectorElement(instance));
            basicStats = instance;

            VisualElement adderUI = infoContainers[INFO_CHARACTER_NAME].Q<VisualElement>("VariblesAdder");
            VariablesAdderUI adder = new(adderUI, statsContainer);
        }

        private void Populate_AddComponents()
        {
            characterComponents.DDFElement.choices.Clear();

            foreach (var type in Enum.GetValues(typeof(ComponentType)))
            {
                characterComponents.DDFElement.choices.Add(type.ToString());
            }

            characterComponents.DDFElement.SetValueWithoutNotify("None");
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

        private void SetCharacterType(ChangeEvent<Enum> evt)
        {
            characterData.characterType = (CharacterType)evt.newValue;
        }

        private void SetClickableButtonColour(int componentIdx)
        {
            creations[componentIdx].NameButton.AddToClassList("ClickableBtn");
            creations[componentIdx].NameButton.style.backgroundColor = new Color(0.4627451f, 0.4627451f, 4627451f);
        }

        private void OpenComponentSettings(int componentIdx)
        {
            var type = (ComponentType)characterComponents[componentIdx].Type;

            string tabName = type switch
            {
                ComponentType.Equipment => INFO_EQUIPMENT_SETTINGS_NAME,
                ComponentType.Health => INFO_HEALTH_SETTINGS_NAME,
                ComponentType.Inventory => INFO_INVENTORY_SETTINGS_NAME,
                _ => null
            };

            if (tabName == null) return;

            switch (type)
            {
                case ComponentType.Equipment:
                    Load_InventoryItemsInEquipment();
                    break;
                case ComponentType.Inventory:
                    break;

                default: break;
            }

            ChangeTab(tabName);
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
            creations.Components.ForEach(c => EnableContainer(c.element, false));
            //ddfAddComponent.SetValueWithoutNotify("None");
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
