using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Editor.TabCharacterEditor;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class CharacterSettings : BaseItemSetting
    {
        const string INFO_HEALTH_SETTINGS_NAME = "HealthSettings";
        const string INFO_EQUIPMENT_SETTINGS_NAME = "EquipmentSettings";
        const string INFO_INVENTORY_SETTINGS_NAME = "InventorySettings";
        const string STATS_CONTAINER_NAME = "infoStats";
        Dictionary<CharacterTab, BaseItemSetting> subTabs = new();
        CharacterData _characterData;

        CharacterTab curTab = CharacterTab.None;
        VisualElement container;
        VisualElement statsContainer;
        StatsVisualizer basicStats = null;

        enum CharacterTab
        {
            None,
            Inventory,
            Equipment,
            Health
        }

        enum VariableType
        {
            None,
            String,
            Int,
            Float,
            Double,
            Enum
        }

        public ComponentsListUI<ElementComponent> Components { get; private set; }
        public EnumModifierUI<CharacterType> EMCharacterType { get; private set; }
        public VariablesAdderUI Adder { get; private set; }
        public EquipmentSettings EquipmentS { get => (EquipmentSettings)subTabs[CharacterTab.Equipment]; }
        public InventorySettings InventoryS { get => (InventorySettings)subTabs[CharacterTab.Inventory]; }

        public override void Initialize(VisualElement container, NameSettings name)
        {
            base.Initialize(container, name);
            this.container = container;

            Components = new ComponentsListUI<ElementComponent>(container);
            Components.OnElementClicked += OpenComponentSettings;
            statsContainer = container.Q<VisualElement>(STATS_CONTAINER_NAME);

            EMCharacterType = new EnumModifierUI<CharacterType>(container.Q<VisualElement>(EnumModifierUI<CharacterType>.ContainerName));
            EMCharacterType.EnumField.RegisterValueChangedCallback(SetCharacterType);

            Populate_AddComponents();
            Populate_EFCharacterType();

            var instance = ScriptableObject.CreateInstance<StatsVisualizer>();
            statsContainer.Add(new InspectorElement(instance));
            basicStats = instance;

            VisualElement adderUI = container.Q<VisualElement>("VariblesAdder");
            Adder = new(adderUI, statsContainer);

            #region Inventory
            Components.DDFElement.RegisterValueChangedCallback((e) => Components.AddElement(e.newValue));
            Components.CreationValidator += ContainsCreation;

            #endregion

            CreateSubTabs();
        }

        private void Populate_AddComponents()
        {
            Components.DDFElement.choices.Clear();

            foreach (var type in Enum.GetValues(typeof(ComponentType)))
            {
                Components.DDFElement.choices.Add(type.ToString());
            }

            Components.DDFElement.SetValueWithoutNotify("None");
        }

        private void Populate_EFCharacterType()
        {
            EMCharacterType.EnumField.Init(CharacterType.None);
        }

        private void SetCharacterType(ChangeEvent<Enum> evt)
        {
            _characterData.characterType = (CharacterType)evt.newValue;
        }

        private void CreateSubTabs()
        {
            VisualElement invtContainer = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"Assets/Proyect/Game/UIToolkit/CharacterEditor/Tabs/{INFO_INVENTORY_SETTINGS_NAME}.uxml").Instantiate();
            subTabs.Add(CharacterTab.Inventory, new InventorySettings());
            var inventory = (InventorySettings)subTabs[CharacterTab.Inventory];
            inventory.Initialize(invtContainer, TxtName);
            inventory.GoBack += () => ChangeTab(CharacterTab.None);
            inventory.OnElementClicked += ChangeTab;

            VisualElement equipContainer = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"Assets/Proyect/Game/UIToolkit/CharacterEditor/Tabs/{INFO_EQUIPMENT_SETTINGS_NAME}.uxml").Instantiate();
            subTabs.Add(CharacterTab.Equipment, new EquipmentSettings());
            subTabs[CharacterTab.Equipment].Initialize(equipContainer, _nameControl);
            ((EquipmentSettings)subTabs[CharacterTab.Equipment]).GoBack += () => ChangeTab(CharacterTab.None);
        }

        private void ChangeTab(ComponentType type)
        {
            if (type == ComponentType.Health)
                ChangeTab(CharacterTab.Health);
        }

        private int? ContainsCreation(IList list, string name)
        {
            var components = (List<ElementComponent>)list;
            int i = 0;
            int? emptyIdx = -1;

            foreach (var component in components)
            {
                if (!component.element.ClassListContains("Disable"))
                {
                    if (component.NameButton.text == name)
                    {
                        return null;
                    }
                }
                else if (!emptyIdx.HasValue)
                {
                    emptyIdx = i;
                }

                ++i;
            }

            return emptyIdx;
        }

        public override void Clear()
        {
            base.Clear();

            TxtName.value = "";
            //CFCreationColor.value = Color.black;
            subTabs[CharacterTab.Inventory].Clear();
            subTabs[CharacterTab.Equipment].Clear();
            EMCharacterType.EnumField.SetValueWithoutNotify(CharacterType.None);

            var instance = ScriptableObject.CreateInstance<StatsVisualizer>();
            statsContainer.Clear();
            statsContainer.Add(new InspectorElement(instance));
            basicStats = instance;
            _characterData = new();
        }

        #region Tab control
        private void ChangeTab(CharacterTab newTab)
        {
            if (curTab == newTab) return;


        }

        private void ClearTabs()
        {

        }

        private void OpenComponentSettings(int componentIdx)
        {
            var type = (ComponentType)Components[componentIdx].Type;

            CharacterTab newTab = type switch
            {
                ComponentType.Equipment => CharacterTab.Equipment,
                ComponentType.Health => CharacterTab.Health,
                ComponentType.Inventory => CharacterTab.Inventory,
                _ => CharacterTab.None
            };

            if (newTab == CharacterTab.None) return;

            switch (type)
            {
                case ComponentType.Equipment:
                    Load_InventoryItemsInEquipment();
                    break;
                case ComponentType.Inventory:
                    break;

                default: break;
            }

            ChangeTab(newTab);
        }

        private void Load_InventoryItemsInEquipment()
        {
            var items = ((InventorySettings)subTabs[CharacterTab.Inventory]).MClInventoryElements;
            ((EquipmentSettings)subTabs[CharacterTab.Equipment]).Load_EquipmentFromList(items);
        }
        #endregion

        #region Variable generation
        private (Type, VisualElement) GenerateVariable(string name) =>
            name switch
            {
                "string" => (typeof(string), new TextField()),
                "int" => (typeof(int), new IntegerField()),
                "float" => (typeof(float), new FloatField()),
                _ => (null, null)
            };

        private void AddVariable(string name)
        {
            (var type, var element) = GenerateVariable(name);

            container.Add(element);
        }

        private void InitializeElement(in VisualElement element)
        {

        }
        #endregion

        public override (InventoryItem item, ItemDataArgs args) GetInfo(ItemDataArgs args)
        {
            return base.GetInfo(args);
        }

        public override bool Check_Changes()
        {
            bool hasChanges = false;

            if (_nameControl.Check_Changes())
            {
                hasChanges = true;
            }


            return hasChanges;
        }

        public override string Save()
        {
            if (!Check_Changes())
                return null;

            //SavingSystem.SaveCreation(ElementType.Character, new CreationData())
            return SaveChanges_Character(_nameControl.name, _id, _nameControl.name);
        }

        public override CreationData Load(ElementType type, string id)
        {
            CreationData? data = SavingSystem.Load(ElementType.Character, _id);

            if (!data.HasValue)
            {
                return default;
            }

            var characterData = (CharacterData)data.Value.data;
            basicStats.stats = _characterData.stats;
            LoadCharacterComponents(in characterData);
            EMCharacterType.EnumField.value = characterData.characterType;

            Components.DDFElement.value = "None";

            return base.Load(type, id);
        }

        private string SaveChanges_Character(string name, string id, string newName = "")
        {
            _characterData.components ??= new();

            AddCharacterComponents();
            //_characterData.color = CFCreationColor.value;
            _characterData.characterType = (CharacterType)EMCharacterType.EnumField.value;
            _characterData.stats = basicStats.stats;
            return SavingSystem.SaveCreation(ElementType.Character, id, new CreationData(name, _characterData));
            //return Save_CreationData(ElementType.Character, name, ref id, _characterData, newName);
        }

        private void AddCharacterComponents()
        {
            var components = from comp in Components.Components
                             where !comp.element.ClassListContains("Disable")
                             select comp;

            foreach (var component in components)
            {
                switch ((ComponentType)component.Type)
                {
                    case ComponentType.Health:
                        var health = ((HealthSettings)subTabs[CharacterTab.Health]).FFHealth.value;
                        
                        _characterData.components[ComponentType.Health] = health;
                        break;

                    case ComponentType.Inventory:
                        AddInventoryComponent();
                        break;

                    case ComponentType.Equipment:
                        AddInventoryComponent();

                        var inventory = (Inventory)_characterData.components[ComponentType.Inventory];
                        _characterData.components[ComponentType.Equipment] =  EquipmentS.GetEquipment(in inventory);
                        break;

                    case ComponentType.None:
                        break;

                    case ComponentType.Dialogue:
                        break;

                    default:
                        _characterData.components[(ComponentType)component.Type] = null;
                        break;
                }
            }
        }

        private void AddInventoryComponent()
        {
            if (_characterData.components.ContainsKey(ComponentType.Inventory))
                return;

            var inventory = ((InventorySettings)subTabs[CharacterTab.Inventory]).GetInventory();
            _characterData.components[ComponentType.Inventory] = inventory;
        }

        private void LoadChanges_Character(string creationId)
        {
            const ElementType type = ElementType.Character;
            CreationData? data = SavingSystem.Load(ElementType.Character, _id);

            if (!data.HasValue)
            {

            }

            CharacterData characterData = (CharacterData)data.Value.data;

            _characterData = characterData;
            basicStats.stats = _characterData.stats;
            LoadCharacterComponents(in characterData);
            EMCharacterType.EnumField.value = characterData.characterType;

            Components.DDFElement.value = "None";
        }

        private void LoadCharacterComponents(in CharacterData data)
        {
            Components.Clear();

            foreach (var component in data.components)
            {
                switch (component.Key)
                {
                    case ComponentType.None:
                        continue;

                    case ComponentType.Health:
                        ((HealthSettings)subTabs[CharacterTab.Health]).FFHealth.value = (float)component.Value;
                        break;

                    case ComponentType.Inventory:
                        LoadInventoryItems((Inventory)component.Value);
                        break;


                    case ComponentType.Equipment:
                        var equipment = (Equipment)component.Value;
                        var elements = EquipmentS.MClEquipmentElements;
                        elements.RestartValues();

                        foreach (var item in equipment.equipment)
                        {
                            Action<ElementCreation> EditData = (e) =>
                            {
                                e.Toggle.value = item.Value.equipped;
                                e.EnumField.value = item.Value.place;
                            };
                            elements.OnElementCreated += (e) => EditData(e);
                            elements.OnElementAdded += (e) => EditData(e);

                            elements.AddElement(item.Key.ToString());

                            elements.OnElementCreated -= (e) => EditData(e);
                            elements.OnElementAdded -= (e) => EditData(e);
                        }
                        break;

                    case ComponentType.Dialogue:
                        break;

                    default:
                        break;
                }

                Components.AddElement(component.Key.ToString());
            }
        }

        private void LoadInventoryItems(in Inventory inventory)
        {
            var elements = InventoryS.MClInventoryElements;
            elements.RestartValues();

            foreach (var item in inventory.items)
            {
                int amount = item.Value;
                Action<ElementCreation> ChangeValue = (e) => elements.ChangeAmount(e.idx, amount);
                elements.OnElementAdded += ChangeValue;
                elements.OnElementCreated += ChangeValue;

                elements.AddElement(item.Key.ToString());

                elements.OnElementAdded -= ChangeValue;
                elements.OnElementCreated -= ChangeValue;
            }
        }
    }
}
