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
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class CharacterSettings : BaseInfoTracker, ISaveable, ISubWindowsContainer
    {
        VisualElement _parent;
        const string STATS_CONTAINER_NAME = "infoStats";
        Dictionary<CharacterTab, SubWindow> subTabs = new();
        CharacterData? _characterData;
        private string _id;

        CharacterTab curTab = CharacterTab.None;
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

        public ComponentsListUI<ElementComponent> ComponentsList { get; private set; }
        public EnumModifierUI<CharacterType> EMCharacterType { get; private set; }
        public VariablesAdderUI Adder { get; private set; }
        public EquipmentSettings EquipmentS { get => (EquipmentSettings)subTabs[CharacterTab.Equipment]; }
        public InventorySettings InventoryS { get => (InventorySettings)subTabs[CharacterTab.Inventory]; }

        public CharacterSettings (VisualElement parent)
        {
            _parent = parent;
        }

        public override void Initialize(VisualElement container, CreationsBaseInfo name)
        {
            base.Initialize(container, name);
            _instance = container;

            ComponentsList = new ComponentsListUI<ElementComponent>(container);
            ComponentsList.OnElementClicked += OpenComponentSettings;
            Setup_EMCharacterType();
            var instance = ScriptableObject.CreateInstance<StatsVisualizer>();
            statsContainer = container.Q<VisualElement>(STATS_CONTAINER_NAME);
            statsContainer.Add(new InspectorElement(instance));
            basicStats = instance;
            VisualElement adderUI = container.Q<VisualElement>("VariblesAdder");
            Adder = new(adderUI, statsContainer);

            Populate_AddComponents();
            //populate_efcharactertype();
            //components.ddfelement.registervaluechangedcallback((e) => components.addelement(e.newvalue));
            //components.creationvalidator += containscreation;

            CreateSubTabs();
        }

        private void Populate_AddComponents()
        {
            ComponentsList.DDFElement.choices.Clear();

            foreach (var name in Enum.GetNames(typeof(ComponentType)))
            {
                ComponentsList.DDFElement.choices.Add(name);
            }

            ComponentsList.DDFElement.value = "None";
        }

        private void Setup_EMCharacterType()
        {
            EMCharacterType = new EnumModifierUI<CharacterType>(_instance.Q<VisualElement>(EnumModifierUI<CharacterType>.ContainerName));
            EMCharacterType.Name.text = "Character Type";
            EMCharacterType.EnumField.Init(CharacterType.None);
        }

        private void CreateSubTabs()
        {
            subTabs.Add(CharacterTab.None, this);
            //var parent = _instance.parent.Q<ScrollView>("infoContainer").Q<VisualElement>("unity-content-container");

            subTabs.Add(CharacterTab.Inventory, new InventorySettings(_parent));
            var inventory = (InventorySettings)subTabs[CharacterTab.Inventory];
            inventory.Initialize(inventory.Instance);
            inventory.GoBack += () => ChangeWindow(CharacterTab.None);
            inventory.OnElementClicked += ChangeTab;
            EnableContainer(inventory.Instance, false);

            subTabs.Add(CharacterTab.Equipment, new EquipmentSettings(_parent));
            var equipment = subTabs[CharacterTab.Equipment];
            equipment.Initialize(equipment.Instance);
            equipment.GoBack += () => ChangeWindow(CharacterTab.None);
            EnableContainer(equipment.Instance, false);

            subTabs.Add(CharacterTab.Health, new HealthSettings());
            subTabs[CharacterTab.Health].Initialize(_parent);
            subTabs[CharacterTab.Health].GoBack += () => ChangeWindow(CharacterTab.None);
            EnableContainer(subTabs[CharacterTab.Health].Instance, false);
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
            if (curTab != CharacterTab.None)
            {
                subTabs[curTab].Clear();
                return;
            }

            TxtName.value = "";
            ComponentsList.Clear();
            subTabs[CharacterTab.Inventory].Clear();
            subTabs[CharacterTab.Equipment].Clear();
            subTabs[CharacterTab.Health].Clear();
            EMCharacterType.Value = CharacterType.None;

            var instance = ScriptableObject.CreateInstance<StatsVisualizer>();
            statsContainer.Clear();
            statsContainer.Add(new InspectorElement(instance));
            basicStats = instance;
            _characterData = null;
            TempName = "";
            _id = null;
        }

        public override void UpdateName()
        {
            TxtName.value = TempName;
        }

        #region Tab control
        private void ChangeTab(ComponentType type)
        {
            if (type == ComponentType.Health)
                ChangeWindow(CharacterTab.Health);
        }

        private void ChangeWindow(CharacterTab newTab)
        {
            if (curTab == newTab) return;

            EnableContainer(subTabs[curTab].Instance, false);
            EnableContainer(subTabs[newTab].Instance, true);
            curTab = newTab;
        }
        #endregion

        private void OpenComponentSettings(int componentIdx)
        {
            var type = (ComponentType)ComponentsList[componentIdx].Type;

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

            ChangeWindow(newTab);
        }

        private void Load_InventoryItemsInEquipment()
        {
            var items = ((InventorySettings)subTabs[CharacterTab.Inventory]).MClInventoryElements;
            ((EquipmentSettings)subTabs[CharacterTab.Equipment]).Load_EquipmentFromList(items);
        }

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

            _instance.Add(element);
        }

        private void InitializeElement(in VisualElement element)
        {

        }
        #endregion

        public override ModificationTypes Check_Changes()
        {
            try
            {
                if (_characterData == null) return CurModificationType = ModificationTypes.Add;

                CurModificationType = ModificationTypes.None;

                if ((_nameControl.Check_Changes() & ModificationTypes.None) == 0)
                {
                    CurModificationType = ModificationTypes.Rename;
                }

                if ((subTabs[CharacterTab.Health].Check_Changes() & ModificationTypes.None) != 0)
                    CurModificationType = ModificationTypes.EditData;

                if ((subTabs[CharacterTab.Inventory].Check_Changes() & ModificationTypes.None) != 0)
                    CurModificationType = ModificationTypes.EditData;

                if ((subTabs[CharacterTab.Equipment].Check_Changes() & ModificationTypes.None) != 0)
                    CurModificationType = ModificationTypes.EditData;

                if (_characterData.Value.stats.speed != basicStats.stats.speed &&
                    _characterData.Value.stats.damageRate != basicStats.stats.Damage &&
                    _characterData.Value.stats.damageRate != basicStats.stats.damageRate &&
                    _characterData.Value.stats.eyesRadious != basicStats.stats.eyesRadious &&
                    _characterData.Value.stats.MinDistance != basicStats.stats.MinDistance
                    )
                {
                    CurModificationType = ModificationTypes.EditData;
                }

                if (_characterData.Value.characterType != EMCharacterType.Value)
                    CurModificationType = ModificationTypes.EditData;

                if (_characterData.Value.components.Count == ComponentsList.Components.Count)
                {
                    foreach (var component in ComponentsList.Components)
                    {
                        if (!_characterData.Value.components.ContainsKey((ComponentType)component.Type))
                        {
                            CurModificationType = ModificationTypes.EditData;
                            break;
                        }
                    }
                }
                else
                    CurModificationType = ModificationTypes.EditData;

                return CurModificationType;
            }
            catch (InvalidExeption e)
            {
                throw e;
            }
        }

        private CharacterData GetInfo()
        {
            CharacterData newData = new();
            newData.components ??= new();
            AddCharacterComponents(ref newData);
            newData.characterType = (CharacterType)EMCharacterType.EnumField.value;
            newData.stats = basicStats.stats;
            newData.characterName = TxtName.value;

            return newData;
        }

        public string Save()
        {
            try
            {
                if ((Check_Changes() & ModificationTypes.None) != 0)
                    return null;

                var newData = GetInfo();
                return SavingSystem.SaveCreation(ElementType.Character, _id, new CreationData(newData.characterName, newData), CurModificationType);
            }
            catch (InvalidExeption e)
            {
                throw e;
            }
        }

        public CreationData Load(ElementType type, string id)
        {
            CreationData? data = SavingSystem.Load(ElementType.Character, id);

            if (!data.HasValue)
            {
                return default;
            }

            LoadInfo((CharacterData)data.Value.data, id);
            Set_CreationState(CreationsState.Editing);

            return data.Value;
        }

        private void LoadInfo(in CharacterData newData, string id)
        {
            _characterData = newData;
            LoadCharacterComponents(in newData);
            ComponentsList.DDFElement.value = "None";
            EMCharacterType.Value = _characterData.Value.characterType;
            basicStats.stats = _characterData.Value.stats;
            TxtName.value = _characterData.Value.characterName;
            TempName = _characterData.Value.characterName;
            _id = id;
        }

        private void AddCharacterComponents(ref CharacterData characterData)
        {
            var components = from comp in ComponentsList.Components
                             where !comp.element.ClassListContains("Disable")
                             select comp;

            foreach (var component in components)
            {
                switch ((ComponentType)component.Type)
                {
                    case ComponentType.Health:
                        var health = ((HealthSettings)subTabs[CharacterTab.Health]).FFHealth.value;

                        characterData.components[ComponentType.Health] = health;
                        break;

                    case ComponentType.Inventory:
                        AddInventoryComponent(ref characterData);
                        break;

                    case ComponentType.Equipment:
                        AddInventoryComponent(ref characterData);

                        var inventory = (Inventory)characterData.components[ComponentType.Inventory];
                        characterData.components[ComponentType.Equipment] = EquipmentS.GetEquipment(in inventory);
                        break;

                    case ComponentType.None:
                        break;

                    case ComponentType.Dialogue:
                        break;

                    default:
                        characterData.components[(ComponentType)component.Type] = null;
                        break;
                }
            }
        }

        private void AddInventoryComponent(ref CharacterData characterData)
        {
            if (characterData.components.ContainsKey(ComponentType.Inventory))
                return;

            var inventory = ((InventorySettings)subTabs[CharacterTab.Inventory]).GetInventory();
            characterData.components[ComponentType.Inventory] = inventory;
        }

        private void LoadCharacterComponents(in CharacterData data)
        {
            ComponentsList.Clear();

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
                        ((InventorySettings)subTabs[CharacterTab.Inventory]).LoadInventoryItems((Inventory)component.Value);
                        break;


                    case ComponentType.Equipment:
                        EquipmentS.LoadEquipment((Equipment)component.Value);
                        break;

                    case ComponentType.Dialogue:
                        break;

                    default:
                        break;
                }

                ComponentsList.AddElement(component.Key.ToString());
            }
        }

        public override void Remove_Changes()
        {
            _nameControl.Remove_Changes();

            if (curTab != CharacterTab.None)
            {
                subTabs[curTab].Remove_Changes();
                return;
            }

            TempName = _nameControl.name;
            CharacterData newInfo = _characterData.Value;
            LoadInfo(newInfo, _id);
        }

        public void CloseWindows()
        {
            for (int i = 1; i < Enum.GetValues(typeof(CharacterTab)).Length; i++)
            {
                EnableContainer(subTabs[(CharacterTab)i].Instance, false);
            }

            curTab = CharacterTab.None;
        }
    }

    public interface ISubWindowsContainer
    {
        public void CloseWindows();
    }
}
