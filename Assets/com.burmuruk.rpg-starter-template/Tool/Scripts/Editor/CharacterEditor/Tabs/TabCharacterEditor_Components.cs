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
        const string infoEquipmentName = "EquipmentSettings";
        const string infoInventoryName = "InventorySettings";

        CreationsBaseInfo nameSettings;
        FloatField ffHealthValue;
        Button btnBackHealthSettings;
        Toggle tglShowElementColour;
        Toggle tglShowCustomColour;

        Dictionary<ElementType, ISaveable> CreationControls = new();
        [SerializeField] List<BuffData> curWeaponsBuffs;
        Dictionary<string, string> tabNames = new();

        private void Setup_Coponents()
        {
            CreationScheduler.creationsNames = GetCreationNames;
            var generalSettings = CreateInstance<GeneralCharacterSettings>();
            CreationControls.Add(ElementType.None, generalSettings);
            generalSettings.Initialize(infoRight, CreationControls);
        }

        private Dictionary<string, string> GetCreationNames(ElementType type)
        {
            Dictionary<string, string> names = new();

            if (!SavingSystem.Data.creations.ContainsKey(type))
            {
                return null;
            }

            foreach (var name in SavingSystem.Data.creations[type])
            {
                names.Add(name.Key, name.Value.Name);
            }

            return names;
        }

        private void Create_ItemTab()
        {
            var settings = CreateInstance<BaseItemSetting>();
            settings.Initialize(infoContainers[INFO_ITEM_SETTINGS_NAME].element, nameSettings);
            CreationControls.Add(ElementType.Item, settings);
        }

        private void Create_WeaponSettings()
        {
            var settings = CreateInstance<WeaponSetting>();
            settings.Initialize(infoContainers[INFO_WEAPON_SETTINGS_NAME].element, nameSettings);
            CreationControls.Add(ElementType.Weapon, settings);
        }

        private void Create_BuffSettings()
        {
            BuffSettings buffSettings = CreateInstance<BuffSettings>();
            buffSettings.Initialize(infoContainers[INFO_BUFF_SETTINGS_NAME].element, nameSettings);
            CreationControls.Add(ElementType.Buff, buffSettings);
        }

        private void Create_ConsumableSettings()
        {
            var settings = CreateInstance<ConsumableSettings>();
            settings.Initialize(infoContainers[INFO_CONSUMABLE_SETTINGS_NAME].element, nameSettings);

            CreationControls.Add(ElementType.Consumable, settings);
        }

        private void Create_ArmourSettings()
        {
            var settings = CreateInstance<ArmourSetting>();
            settings.Initialize(infoContainers[INFO_ARMOUR_SETTINGS_NAME].element, nameSettings);
            CreationControls.Add(ElementType.Armour, settings);
        }

        private void Create_GeneralCharacterSettings()
        {
            tglShowElementColour = infoContainers[INFO_GENERAL_SETTINGS_CHARACTER_NAME].element.Q<Toggle>();
            tglShowElementColour.RegisterValueChangedCallback(OnValueChanged_TGLElementColour);

            tglShowCustomColour = infoContainers[INFO_GENERAL_SETTINGS_CHARACTER_NAME].element.Q<Toggle>();
            tglShowCustomColour.RegisterValueChangedCallback(OnValueChanged_TGLCustomColour);
        }

        private void OnValueChanged_TGLCustomColour(ChangeEvent<bool> evt)
        {
            searchBar.ShowCustomColour = evt.newValue;
        }

        private void OnValueChanged_TGLElementColour(ChangeEvent<bool> evt)
        {
            searchBar.ShowElementColour = evt.newValue;
        }
    }
}
