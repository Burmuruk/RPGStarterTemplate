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
        ListOfBuffsVisualizer weaponBuffs;
        List<BaseItemSetting> settingsElements = new();
        [SerializeField] List<BuffData> curWeaponsBuffs;

        private void Create_ItemTab()
		{
            //curItemData = ScriptableObject.CreateInstance<InventoryItem>();
            //infoContainers[infoItemSettingsName].Add(new InspectorElement(curItemData));

            var settings = new BaseItemSetting();
            settings.Initialize(infoContainers[infoItemSettingsName]);
        }

        private class ListOfBuffsVisualizer : ScriptableObject
        {
            public EnumField buffType;
            public int testInt;
            public List<BuffData> list;
        }

        struct BuffTuneado
        {

            BuffData data;
        }

        private void Create_WeaponSettings()
        {
            curWeaponData = ScriptableObject.CreateInstance<Weapon>();
            weaponBuffs = CreateInstance<ListOfBuffsVisualizer>();
            infoContainers[infoWeaponSettingsName].Add(new InspectorElement(weaponBuffs));

            var settings = new WeaponSetting();
            settings.Initialize(infoContainers[infoWeaponSettingsName]);
            settingsElements.Add(settings);

            Button btnAdd = new Button();
            btnAdd.text = "Add";
            
            Foldout foldout = new Foldout()
            {
                
            };

            VisualTreeAsset ElementTag = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Proyect/Game/UIToolkit/CharacterEditor/Tabs/ItemInfoBase.uxml");
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Proyect/Game/UIToolkit/Styles/LineTags.uss");
            infoContainers[infoWeaponSettingsName].styleSheets.Add(styleSheet);
            //foldout.Add(ElementTag.Instantiate());

            var weaponType = infoContainers[infoWeaponSettingsName].Q<VisualElement>("TypeAdderWeapon");
            weaponType.Q<Label>().text = "Weapon type";


            ListView listView = new ListView();
            listView.itemsSource = curWeaponsBuffs;
            listView.makeItem = () => new Label("Hi");
            listView.bindItem = (e, i) => (e as Label).text = i.ToString();

            TreeView treeView = new TreeView();
            btnAdd.clicked += () => { curWeaponsBuffs ??= new(); curWeaponsBuffs.Add(new BuffData()); Debug.Log(curWeaponsBuffs.Count); listView.RefreshItems(); ; };
            treeView.makeItem = () => new Label();
            treeView.bindItem = (e, i) => (e as Label).text = treeView.GetItemDataForIndex<BuffData>(i).stat.ToString();
            
            foldout.Add(treeView);
            infoContainers[infoWeaponSettingsName].Add(btnAdd);
            infoContainers[infoWeaponSettingsName].Add(listView);
            infoContainers[infoWeaponSettingsName].Add(foldout);
            //infoContainers[infoWeaponSettingsName].Add(new InspectorElement(curWeaponData));

            //rootVisualElement.Add(list);
        }

        private void Create_BuffSettings()
        {
            curBuffData = ScriptableObject.CreateInstance<BuffVisulizer>();
            infoContainers[infoBuffSettingsName].Add(new InspectorElement(curBuffData));
        }

        private void Create_ConsumableSettings()
        {
            curConsumableData = ScriptableObject.CreateInstance<ConsumableItem>();
            infoContainers[infoConsumableSettingsName].Add(new InspectorElement(curConsumableData));

            var settings = new ConsumableSettings();
            settings.Initialize(infoContainers[infoConsumableSettingsName]);
            settingsElements.Add(settings);
        }

        private void Create_ArmorSettings()
        {
            curArmorData = ScriptableObject.CreateInstance<ArmourElement>();
            infoContainers[infoArmorSettingsName].Add(new InspectorElement(curArmorData));

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
