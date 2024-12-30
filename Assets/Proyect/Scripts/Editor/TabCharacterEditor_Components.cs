using Burmuruk.Tesis.Combat;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
        List<BaseItemSetting> settingsElements = new();
        [SerializeField] List<BuffData> curWeaponsBuffs;

        private void Create_ItemTab()
		{
            //curItemData = ScriptableObject.CreateInstance<InventoryItem>();
            //infoContainers[infoItemSettingsName].Add(new InspectorElement(curItemData));

            var settings = new BaseItemSetting();
            settings.Initialize(infoContainers[infoItemSettingsName]);
        }

        private void Create_WeaponSettings()
        {
            var settings = new WeaponSetting();
            settings.Initialize(infoContainers[infoWeaponSettingsName]);
            settingsElements.Add(settings);

            var bodyPart = infoContainers[infoWeaponSettingsName].Q<EnumField>("efBodyPart");
            bodyPart.Init(EquipmentType.None);

            var weaponType = infoContainers[infoWeaponSettingsName].Q<VisualElement>("TypeAdderWeapon");
            weaponType.Q<Label>().text = "Weapon type";
            weaponType.Q<EnumField>().Init(WeaponType.None);
        }

        private void Create_BuffSettings()
        {
            curBuffData = ScriptableObject.CreateInstance<BuffVisulizer>();
            infoContainers[infoBuffSettingsName].Add(new InspectorElement(curBuffData));
        }

        private void Create_ConsumableSettings()
        {
            curConsumableData = ScriptableObject.CreateInstance<ConsumableItem>();
            //infoContainers[infoConsumableSettingsName].Add(new InspectorElement(curConsumableData));
            
            var settings = new ConsumableSettings();
            settings.Initialize(infoContainers[infoConsumableSettingsName]);
            settingsElements.Add(settings);
        }

        private void Create_ArmourSettings()
        {
            var settings = new ArmourSetting();
            settings.Initialize(infoContainers[infoArmorSettingsName]);
            settingsElements.Add(settings);
        }

        private void Create_HealthSettings()
        {
            ffHealthValue = infoContainers[infoHealthSettingsName].Q<FloatField>();
            ffHealthValue.RegisterValueChangedCallback(OnValueChanged_FFHealthValue);

            btnBackHealthSettings = infoContainers[infoHealthSettingsName].Q<Button>();
            
            btnBackHealthSettings.clicked += () => ChangeTab(lastTab);
        }

        private void Create_GeneralCharacterSettings()
        {
            tglShowElementColour = infoContainers[infoGeneralSettinsCharacterName].Q<Toggle>();
            tglShowElementColour.RegisterValueChangedCallback(OnValueChanged_TGLElementColour);

            tglShowCustomColour = infoContainers[infoGeneralSettinsCharacterName].Q<Toggle>();
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
