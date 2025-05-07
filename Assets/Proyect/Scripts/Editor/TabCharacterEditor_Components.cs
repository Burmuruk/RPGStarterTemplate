using Burmuruk.Tesis.Editor.Controls;
using Burmuruk.Tesis.Stats;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public partial class TabCharacterEditor : BaseLevelEditor
    {
        const string infoHealthName = "HealthSettings";
        NameSettings nameSettings;
        FloatField ffHealthValue;
        Button btnBackHealthSettings;

        Toggle tglShowElementColour;
        Toggle tglShowCustomColour;

        Dictionary<ElementType, BaseItemSetting> settingsElements = new();
        [SerializeField] List<BuffData> curWeaponsBuffs;
        Dictionary<string, string> tabNames = new();

        private void Setup_Coponents()
        {
            nameSettings = new NameSettings("");

            CreationScheduler.creationsNames = GetCreationNames;
            OnCreationModified += (modification, type, id, data) =>
            {
                var info = new BaseCreationInfo(id, data.Name, data);
                CreationScheduler.ChangeData(modification, type, id, info);
            };
        }

        private Dictionary<string, string> GetCreationNames(ElementType type)
        {
            Dictionary<string, string> names = new();

            foreach (var name in SavingSystem.Data.creations[type])
            {
                names.Add(name.Key, name.Value.Name);
            }

            return names;
        }

        private void Create_ItemTab()
        {
            var settings = new BaseItemSetting();
            settings.Initialize(infoContainers[INFO_ITEM_SETTINGS_NAME], nameSettings);
            settingsElements.Add(ElementType.Item, settings);
        }

        private void Create_WeaponSettings()
        {
            var settings = new WeaponSetting();
            settings.Initialize(infoContainers[INFO_WEAPON_SETTINGS_NAME], nameSettings);
            settingsElements.Add(ElementType.Weapon, settings);

            //OnCreationAdded += (type, item) =>
            //{
            //    if (type == ElementType.Buff)
            //    {
            //        UpdateBuffEnumTypes(settings.BuffAdder);
            //    }
            //};
        }

        private void Create_BuffSettings()
        {
            BuffSettings buffSettings = new BuffSettings();
            buffSettings.Initialize(container, nameSettings);

            //editingData[ElementType.Buff].data = ScriptableObject.CreateInstance<BuffVisulizer>();
            //var curBuff = editingData[ElementType.Buff].data as BuffVisulizer;

            //infoContainers[INFO_BUFF_SETTINGS_NAME].Add(new InspectorElement(curBuff));
        }

        private void Create_ConsumableSettings()
        {
            //curConsumableData = ScriptableObject.CreateInstance<ConsumableItem>();
            //infoContainers[INFO_CONSUMABLE_SETTINGS_NAME].Add(new InspectorElement(curConsumableData));

            var settings = new ConsumableSettings();
            settings.Initialize(infoContainers[INFO_CONSUMABLE_SETTINGS_NAME], nameSettings);

            settingsElements.Add(ElementType.Consumable, settings);
        }

        private void Create_ArmourSettings()
        {
            var settings = new ArmourSetting();
            settings.Initialize(infoContainers[INFO_ARMOUR_SETTINGS_NAME], nameSettings);
            settingsElements.Add(ElementType.Armour, settings);
        }

        private void Create_GeneralCharacterSettings()
        {
            tglShowElementColour = infoContainers[INFO_GENERAL_SETTINGS_CHARACTER_NAME].Q<Toggle>();
            tglShowElementColour.RegisterValueChangedCallback(OnValueChanged_TGLElementColour);

            tglShowCustomColour = infoContainers[INFO_GENERAL_SETTINGS_CHARACTER_NAME].Q<Toggle>();
            tglShowCustomColour.RegisterValueChangedCallback(OnValueChanged_TGLCustomColour);
        }

        private void OnValueChanged_TGLCustomColour(ChangeEvent<bool> evt)
        {
            Show_ElementsColours(tglShowCustomColour);
        }

        private void OnValueChanged_TGLElementColour(ChangeEvent<bool> evt)
        {
            Show_ElementsColours(tglShowElementColour);
        }

        private void Show_ElementsColours(Toggle toggle)
        {
            toggle.value = !toggle.value;

            SearchAllElements();
        }
    }
}
