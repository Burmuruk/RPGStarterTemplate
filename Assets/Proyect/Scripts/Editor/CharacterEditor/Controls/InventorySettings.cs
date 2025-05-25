using Burmuruk.Tesis.Editor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class InventorySettings : SubWindow, IUIListContainer<BaseCreationInfo>
    {
        const string INFO_INVENTORY_SETTINGS_NAME = "InventorySettings";
        Inventory _changes = default;
        Dictionary<(string name, string type), string> _DropDownIds = new();
        string _selectedType;

        ElementType[] inventoryChoices = new ElementType[]
        {
            ElementType.None,
            ElementType.Item,
            ElementType.Consumable,
            ElementType.Weapon,
            ElementType.Armour,
            ElementType.Ability,
        };

        public event Action<ComponentType> OnElementClicked;

        public Button btnBackInventorySettings { get; private set; }
        public ComponentsListUI<ElementCreation> MClInventoryElements { get; private set; }

        public InventorySettings(VisualElement container)
        {
            _container = container;
            _instance = UtilitiesUI.CreateDefaultTab(INFO_INVENTORY_SETTINGS_NAME);
            _container.Add(_instance);
        }

        public override void Initialize(VisualElement container)
        {
            base.Initialize(container);

            btnBackInventorySettings = _instance.Q<Button>();
            btnBackInventorySettings.clicked += () => GoBack?.Invoke();
            _instance.Q<VisualElement>(ComponentsList.CONTAINER_NAME);
            MClInventoryElements = new ComponentsListUI<ElementCreation>(_instance);
            Setup_ComponentsList();
            
            Populate_DDFType();
            Populate_DDFElement();
            RegisterToChanges();
            //MultiColumnListView lstInventory = new MultiColumnListView();
        }

        private void Setup_ComponentsList()
        {
            MClInventoryElements.OnElementCreated += Setup_Element;
            MClInventoryElements.OnElementAdded += Add_ElementId;
            MClInventoryElements.AddElementExtraData += Add_ElementId;
            MClInventoryElements.DeletionValidator += TryRemove_Element;
            MClInventoryElements.DDFType.RegisterValueChangedCallback((evt) => OnValueChanged_EFInventoryType(evt));
            MClInventoryElements.DDFElement.RegisterValueChangedCallback((evt) => OnValueChanged_DDFInventoryElement(evt.newValue));
            MClInventoryElements.OnElementClicked += (idx) =>
            {
                var type = (ComponentType)MClInventoryElements.Components[idx].Type;
                OnElementClicked(type);
            };
        }

        private void Setup_Element(ElementCreation creation)
        {
            EnableContainer(creation.IFAmount, true);
        }

        private void Add_ElementId(ElementCreation element)
        {
            element.Id = _DropDownIds[(element.NameButton.text, element.Type.ToString())];
        }

        #region Traking changes
        private void RegisterToChanges()
        {
            ElementType[] elements = new ElementType[]
            {
                ElementType.Item,
                ElementType.Consumable,
                ElementType.Weapon,
                ElementType.Armour,
            };

            foreach (var element in elements)
                CreationScheduler.Add(ModificationTypes.Rename, element, this);

            foreach (var element in elements)
                CreationScheduler.Add(ModificationTypes.Remove, element, this);

            foreach (var element in elements)
                CreationScheduler.Add(ModificationTypes.Add, element, this);
        }

        public virtual void AddData(in BaseCreationInfo newValue)
        {
            Populate_DDFType();
            Populate_DDFElement();
        }

        public virtual void RenameCreation(in BaseCreationInfo newValue)
        {
            Populate_DDFElement();

            foreach (var component in MClInventoryElements.Components)
            {
                if (component.Id == newValue.Id)
                {
                    component.NameButton.text = newValue.Name;
                    return;
                }
            }
        }

        public virtual void RemoveData(in BaseCreationInfo newValue)
        {
            Populate_DDFType();
            Populate_DDFElement();

            foreach (var component in MClInventoryElements.Components)
            {
                if (component.Id == newValue.Id)
                {
                    MClInventoryElements.RemoveComponent(component.idx);
                    return;
                }
            }
        }
        #endregion

        private void OnValueChanged_EFInventoryType(ChangeEvent<string> evt)
        {
            Populate_DDFElement();
            _selectedType = evt.newValue;
        }

        private void OnValueChanged_DDFInventoryElement(string name)
        {
            if (name == "None") return;

            int? elementIdx = Check_HasInventoryComponent(name);

            if (elementIdx.HasValue)
            {
                MClInventoryElements.IncrementElement(elementIdx.Value);
                return;
            }

            MClInventoryElements.AddElement(name, MClInventoryElements.DDFType.value);
        }

        private void Populate_DDFType()
        {
            MClInventoryElements.DDFType.choices.Clear();
            MClInventoryElements.DDFElement.choices.Clear();
            _DropDownIds.Clear();

            foreach (var type in inventoryChoices)
            {
                if (!SavingSystem.Data.creations.ContainsKey(type))
                    continue;

                string typeName = type.ToString();
                MClInventoryElements.DDFType.choices.Add(type.ToString());

                foreach (var creation in SavingSystem.Data.creations[type])
                {
                    _DropDownIds.Add((creation.Value.Name, typeName), creation.Key);
                }
            }

            MClInventoryElements.DDFType.SetValueWithoutNotify(Get_SelectedType());
        }

        private void Populate_DDFElement()
        {
            var value = MClInventoryElements.DDFType.value;

            if (!Verify_DDFType(value, out var type))
            {
                MClInventoryElements.DDFElement.SetValueWithoutNotify("None");
                return;
            }

            MClInventoryElements.DDFElement.choices.Clear();

            foreach (var creation in SavingSystem.Data.creations[type])
            {
                MClInventoryElements.DDFElement.choices.Add(creation.Value.Name);
            }

            MClInventoryElements.DDFElement.SetValueWithoutNotify("None");
        }

        /// <summary>
        /// Verifies that the current value exists between the saved creations.
        /// </summary>
        /// <param name="value">Current Value.</param>
        /// <param name="type">Selected type.</param>
        /// <returns></returns>
        private bool Verify_DDFType(string value, out ElementType type)
        {
            type = ElementType.None;

            if (string.IsNullOrEmpty(value) || value == "None")
                return false;

            if (!Enum.TryParse(value, out type)) return false;

            if (!SavingSystem.Data.creations.ContainsKey(type)) return false;

            return true;
        }

        private string Get_SelectedType()
        {
            if (string.IsNullOrEmpty(_selectedType) || _selectedType == "None")
                return "None";

            foreach (var item in _DropDownIds)
            {
                if (item.Key.type.ToString() == _selectedType)
                {
                    return item.Key.type.ToString();
                }
            }

            _selectedType = string.Empty;
            return "None";
        }

        private int? Check_HasInventoryComponent(string name)
        {
            for (int i = 0; i < MClInventoryElements.Components.Count; i++)
            {
                if (!MClInventoryElements.Components[i].element.ClassListContains("Disable") &&
                    MClInventoryElements.Components[i].NameButton.text.Contains(name))
                    return i;
            }

            return null;
        }

        public Inventory GetInventory()
        {
            var inventory = new Inventory();
            inventory.items = new();

            for (int i = 0; i < MClInventoryElements.Components.Count; i++)
            {
                if (MClInventoryElements[i].element.ClassListContains("Disable"))
                    continue;

                var curElement = MClInventoryElements[i];
                string type = curElement.Type.ToString();
                string id = _DropDownIds[(curElement.NameButton.text, type)];
                inventory.items[id] = MClInventoryElements.Amounts[i];
            }

            return inventory;
        }

        public void LoadInventoryItems(in Inventory inventory)
        {
            var elements = MClInventoryElements;
            elements.RestartValues();
            var newInventory = inventory;

            if (inventory.items != null)
                foreach (var item in inventory.items)
                {
                    int amount = item.Value;

                    if (!SavingSystem.Data.TryGetCreation(item.Key, out var data, out var type))
                        continue;

                    Action<ElementCreation> ChangeValue = (e) => elements.ChangeAmount(e.idx, item.Value);
                    elements.OnElementAdded += ChangeValue;
                    elements.OnElementCreated += ChangeValue;

                    if (!elements.AddElement(data.Name, type.ToString()))
                        newInventory.items.Remove(item.Key);

                    elements.OnElementAdded -= ChangeValue;
                    elements.OnElementCreated -= ChangeValue;
                }

            _changes = newInventory;
        }

        public override void Clear()
        {
            MClInventoryElements.Clear();
            _selectedType = null;
        }

        public override ModificationTypes Check_Changes()
        {
            var inventory = GetInventory();

            if (_changes.items == null) return CurModificationType = ModificationTypes.None;

            foreach (var item in inventory.items)
            {
                if (!_changes.items.ContainsKey(item.Key) || _changes.items[item.Key] != item.Value)
                    return ModificationTypes.EditData;
            }

            return ModificationTypes.None;
        }

        public override void Remove_Changes()
        {
            var newData = _changes;
            LoadInventoryItems(newData);
        }

        #region Components List

        private bool TryRemove_Element(int idx)
        {
            if (((ComponentType)MClInventoryElements[idx].Type) == ComponentType.Inventory)
            {
                foreach (var component in MClInventoryElements.Components)
                {
                    if (!component.element.ClassListContains("Disable"))
                    {
                        if ((ComponentType)component.Type == ComponentType.Equipment)
                        {
                            Notify("Equipment requires an Inventory component to store the items", BorderColour.Error);
                            return false;
                        }
                    }
                    else
                        break;
                }
            }

            return true;
        }
        #endregion
    }
}
