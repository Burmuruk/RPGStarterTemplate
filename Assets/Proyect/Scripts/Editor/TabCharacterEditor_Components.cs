using Burmuruk.Tesis.Combat;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public class BuffVisulizer : ScriptableObject
    {
        [SerializeField] public BuffData buff;
    }

    public partial class TabCharacterEditor : BaseLevelEditor
    {
        const string infoHealthName = "HealthSettings";

        FloatField ffHealthValue;
        Button btnBackHealthSettings;

        Toggle tglShowElementColour;
        Toggle tglShowCustomColour;

        InventoryItem curItemData;
        BuffVisulizer curBuffData;
        Weapon curWeaponData;
        ConsumableItem curConsumableData;
        ArmourElement curArmorData;
        Dictionary<ElementType, BaseItemSetting> settingsElements = new();
        [SerializeField] List<BuffData> curWeaponsBuffs;
        Dictionary<string, string> tabNames = new();
        EnumScheduler enumScheduler = new EnumScheduler();

        private void Create_ItemTab()
        {
            var settings = new BaseItemSetting();
            settings.Initialize(infoContainers[INFO_ITEM_SETTINGS_NAE], txtNameCreation);
            settingsElements.Add(ElementType.Item, settings);
        }

        private void Create_WeaponSettings()
        {
            var settings = new WeaponSetting();
            settings.Initialize(infoContainers[INFO_WEAPON_SETTINGS_NAME], txtNameCreation);
            settingsElements.Add(ElementType.Weapon, settings);

            OnCreationAdded += (type, item) =>
            {
                if (type == ElementType.Buff)
                {
                    UpdateBuffEnumTypes(settings.BuffAdder);
                }
            };
        }

        private void UpdateBuffEnumTypes(BuffAdderUI adder)
        {
            var values = new List<string>();

            foreach (var creation in charactersLists.creations[ElementType.Buff])
            {
                values.Add(creation.Value.Name);
            }

            adder.SetBuffs(values);
        }

        private void Create_BuffSettings()
        {
            curBuffData = ScriptableObject.CreateInstance<BuffVisulizer>();
            infoContainers[INFO_BUFF_SETTINGS_NAME].Add(new InspectorElement(curBuffData));
        }

        private void Create_ConsumableSettings()
        {
            //curConsumableData = ScriptableObject.CreateInstance<ConsumableItem>();
            //infoContainers[INFO_CONSUMABLE_SETTINGS_NAME].Add(new InspectorElement(curConsumableData));

            var settings = new ConsumableSettings();
            settings.Initialize(infoContainers[INFO_CONSUMABLE_SETTINGS_NAME], txtNameCreation);
            settingsElements.Add(ElementType.Consumable, settings);
        }

        private void Create_ArmourSettings()
        {
            var settings = new ArmourSetting();
            settings.Initialize(infoContainers[INFO_ARMOUR_SETTINGS_NAME], txtNameCreation);
            settingsElements.Add(ElementType.Armour, settings);
        }

        private void Create_HealthSettings()
        {
            ffHealthValue = infoContainers[INFO_HEALTH_SETTINGS_NAME].Q<FloatField>();
            ffHealthValue.RegisterValueChangedCallback(OnValueChanged_FFHealthValue);

            btnBackHealthSettings = infoContainers[INFO_HEALTH_SETTINGS_NAME].Q<Button>();

            btnBackHealthSettings.clicked += () => ChangeTab(lastTab);
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

        private void OnValueChanged_FFHealthValue(ChangeEvent<float> evt)
        {
            AddComponentData(ComponentType.Health);
        }

        private void AddComponentData(ComponentType type)
        {
            object item = null;

            item = type switch
            {
                ComponentType.Health => new Health()
                {
                    HP = (int)ffHealthValue.value
                },
                _ => null
            };

            characterData.components.TryAdd(type, item);
        }
    }
}
