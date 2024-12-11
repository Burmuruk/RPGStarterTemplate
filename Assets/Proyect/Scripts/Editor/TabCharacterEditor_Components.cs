using Burmuruk.Tesis.Combat;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
	public class BuffVisulizer : ScriptableObject
	{
		[SerializeField] BuffData buff;
	}

    public partial class TabCharacterEditor : BaseLevelEditor
	{
        const string infoHealthName = "HealthSettings";

        FloatField ffHealthValue;
        Button btnBackHealthSettings;

        Toggle tglShowElementColour;
        Toggle tglShowCustomColour;

        InventoryItem curItemData;
        BuffData curBuffData;
        Weapon curWeaponData;
        ConsumableItem curConsumableData;
        ArmorElement curArmorData;

        private void Create_ItemTab()
		{
            var instance = ScriptableObject.CreateInstance<InventoryItem>();
            infoContainers[infoItemSettingsName].Add(new InspectorElement(instance));
        }

        private void Create_WeaponSettings()
        {
            var instance = ScriptableObject.CreateInstance<Weapon>();
            infoContainers[infoWeaponSettingsName].Add(new InspectorElement(instance));
        }

        private void Create_BuffSettings()
        {
            var instance = ScriptableObject.CreateInstance<BuffVisulizer>();
            infoContainers[infoBuffSettingsName].Add(new InspectorElement(instance));
        }

        private void Create_ConsumableSettings()
        {
            var instance = ScriptableObject.CreateInstance<ConsumableItem>();
            infoContainers[infoConsumableSettingsName].Add(new InspectorElement(instance));
        }

        private void Create_ArmorSettings()
        {
            var instance = ScriptableObject.CreateInstance<ArmorElement>();
            infoContainers[infoArmorSettingsName].Add(new InspectorElement(instance));
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
