using Burmuruk.Tesis.Editor.Controls;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor
{
    public partial class TabCharacterEditor : BaseLevelEditor
    {
        const string TXT_CREATION_NAME = "txtName";
        const string CREATION_COLOUR_NAME = "cfSettingColour";
        ColorField CFCreationColor;
        Button btnSettingAccept;
        Button btnSettingCancel;

        const string COMPONENTS_CONTAINER_NAME = "componentsConatiner";
        const string SETTINGS_COLOUR_CONTAINER = "ColourContainer";
        const string DDF_ADD_COMPONENT_NAME = "ddfElement";
        const string BTN_GO_BACK_SETTINGS = "btnGoBack";
        const string INFO_EXTRA_SETTINGS_NAME = "infoContainer";

        VisualElement infoExtraSetting;
        VisualElement componentsContainer;
        DropdownField ddfAddComponent;

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
            CharacterSettings characterSettings = new CharacterSettings();
            characterSettings.Initialize(infoContainers[INFO_CHARACTER_NAME], nameSettings);

            settingsElements.Add(ElementType.Character, characterSettings);
        }

        private void OnCancel_BtnSetting()
        {
            ElementType type = currentSettingTag.type;
            ((IChangesObserver)settingsElements[type]).Remove_Changes();

            //switch (currentSettingTag.type)
            //{
            //    case ElementType.Character:
            //        settingsElements[ElementType.Character].DiscardChanges();
            //        break;
            //    case ElementType.Weapon:
            //    case ElementType.Consumable:
            //    case ElementType.Item:
            //    case ElementType.Armour:

            //        if (settingsState != SettingsState.Editing)
            //            nameSettings.TxtName.value = "";

            //        CFCreationColor.value = Color.black;

            //        (settingsElements[currentSettingTag.type] as IClearable).Clear();
            //        break;

            //    case ElementType.Buff:
            //        CurBuffData.data.creationId = null;
            //        CurBuffData.visualizer.buff = default;
            //        EditorUtility.SetDirty(CurBuffData.visualizer);
            //        break;

            //    default: break;
            //}
        }

        private void OnAccept_BtnAccept()
        {
            switch (settingsState)
            {
                case SettingsState.None:
                    break;

                case SettingsState.Creating:
                    if (!Save_Creation())
                        return;
                    break;

                case SettingsState.Editing:
                    if (!Edit_Creation())
                        return;
                    break;

                default:
                    break;
            }

            OnCancel_BtnSetting();

            settingsState = SettingsState.None;
            EnableContainer(infoSetup, false);
            Highlight(btnsRight_Tag[currentSettingTag.idx].element, false);
            currentSettingTag = (ElementType.None, -1);
            editingElement = (ElementType.None, "", -1);

            SearchAllElements();
        }

        private bool Edit_Creation()
        {
            ElementType type = currentSettingTag.type;
            string result = settingsElements[type].Save();

            if (result != null)
            {
                Notify(result, BorderColour.Error);
                return false;
            }

            Notify("Changes saved", BorderColour.Approved);
            return true;

            //switch (type)
            //{
            //    case ElementType.Character:
            //        result = SaveChanges_Character(editingElement.name, curData.creationId, txtNameCreation.value);
            //        break;

            //    case ElementType.Buff:

            //        var visualizer = curData.data as BuffVisulizer;

            //        //if (!settingsElements[currentSettingTag.type].CheckChanges(visualizer.buff, out List<VisualElement> changes))
            //        //    return false;

            //        var buff = charactersLists.creations[ElementType.Buff][curData.creationId];

            //        result = Save_CreationData(ElementType.Buff, buff.Name, ref curData.creationId, visualizer.buff, txtNameCreation.text);
            //        break;

            //    case ElementType.Item:
            //    case ElementType.Armour:

            //        var creationData = (InventoryItem)editingData[type].data;

            //        if (!settingsElements[currentSettingTag.type].CheckChanges(creationData, null, out List<VisualElement> changes, out modType))
            //            return false;

            //        result = Save_Creation();
            //        break;

            //    case ElementType.Weapon:
            //    case ElementType.Consumable:

            //        (var item, var args) = ((InventoryItem, BuffsNamesDataArgs))editingData[type].data;

            //        if (!settingsElements[currentSettingTag.type].CheckChanges(item, args, out List<VisualElement> bChanges, out modType))
            //            return false;

            //        newData = type switch
            //        {
            //            ElementType.Item => settingsElements[ElementType.Item].GetInfo(null).item,
            //            ElementType.Weapon => GetBuffsIds(ElementType.Weapon),
            //            ElementType.Armour => settingsElements[ElementType.Armour].GetInfo(null).item,
            //            ElementType.Consumable => GetBuffsIds(ElementType.Consumable),
            //            _ => null
            //        };

            //        result = Save_CreationData(type, curData.name, ref curData.creationId, newData, curData.name);

            //        break;

            //    default: break;
            //}

            //if (result)
            //{
            //    OnCreationModified?.Invoke(modType, type, curData.creationId, new CreationData(curData.name, newData));
            //    Notify("Element edited.", BorderColour.Approved);
            //}
            //else
            //    return false;

            //return true;
        }

        private void SetClickableButtonColour(int componentIdx)
        {
            creations[componentIdx].NameButton.AddToClassList("ClickableBtn");
            creations[componentIdx].NameButton.style.backgroundColor = new Color(0.4627451f, 0.4627451f, 4627451f);
        }
    }
}
