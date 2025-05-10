using Burmuruk.Tesis.Editor.Utilities;
using System;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class InventorySettings : SubWindow
    {
        const string INFO_INVENTORY_SETTINGS_NAME = "InventorySettings";
        Inventory _changes = default;

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

        public override void Initialize(VisualElement container, NameSettings name)
        {
            base.Initialize(container, name);

            var control = UtilitiesUI.CreateDefaultTab(INFO_INVENTORY_SETTINGS_NAME);
            _container.Add(control);
            btnBackInventorySettings = container.Q<Button>();
            btnBackInventorySettings.clicked += () => GoBack?.Invoke();
            container.Q<VisualElement>(ComponentsList.CONTAINER_NAME);
            MClInventoryElements = new ComponentsListUI<ElementCreation>(container);
            MClInventoryElements.DDFType.RegisterValueChangedCallback((evt) => OnValueChanged_EFInventoryType(evt));
            MClInventoryElements.DDFType.SetValueWithoutNotify("None");
            MClInventoryElements.DDFElement.RegisterValueChangedCallback((evt) => OnValueChanged_DDFInventoryElement(evt.newValue));
            MClInventoryElements.DDFElement.SetValueWithoutNotify("None");
            MClInventoryElements.OnElementClicked += (idx) =>
            {
                var type = (ComponentType)MClInventoryElements.Components[idx].Type;
                OnElementClicked(type);
            };
            Populate_DDFsInventory();

            //MultiColumnListView lstInventory = new MultiColumnListView();
        }

        private void OnValueChanged_EFInventoryType(ChangeEvent<string> evt)
        {
            MClInventoryElements.DDFElement.choices.Clear();
            var type = Enum.Parse<ElementType>(evt.newValue);
            MClInventoryElements.DDFElement.SetValueWithoutNotify("None");

            if (!SavingSystem.Data.creations.ContainsKey(type))
                return;

            foreach (var creation in SavingSystem.Data.creations[type].Values)
            {
                MClInventoryElements.DDFElement.choices.Add(creation.Name);
            }
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

            MClInventoryElements.AddElement(name);
        }

        private void Populate_DDFsInventory()
        {
            MClInventoryElements.DDFType.choices.Clear();
            MClInventoryElements.DDFElement.choices.Clear();

            foreach (var type in inventoryChoices)
            {
                MClInventoryElements.DDFType.choices.Add(type.ToString());

                if (!SavingSystem.Data.creations.ContainsKey(type))
                    continue;

                foreach (var creation in SavingSystem.Data.creations[type])
                {
                    MClInventoryElements.DDFElement.choices.Add(creation.Value.Name);
                }
            }
        }

        private int? Check_HasInventoryComponent(string name)
        {
            for (int i = 0; i < MClInventoryElements.Components.Count; i++)
            {
                if (!MClInventoryElements.Components[i].element.ClassListContains("Disable") && MClInventoryElements.Components[i].NameButton.text.Contains(name))
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

                inventory.items.TryAdd((ElementType)MClInventoryElements[i].Type, MClInventoryElements.Amounts[i]);
            }

            return inventory;
        }

        public void LoadInventoryItems(in Inventory inventory)
        {
            var elements = MClInventoryElements;
            elements.RestartValues();
            var newInventory = inventory;

            foreach (var item in inventory.items)
            {
                int amount = item.Value;
                Action<ElementCreation> ChangeValue = (e) => elements.ChangeAmount(e.idx, amount);
                elements.OnElementAdded += ChangeValue;
                elements.OnElementCreated += ChangeValue;

                if (!elements.AddElement(item.Key.ToString()))
                    newInventory.items.Remove(item.Key);

                elements.OnElementAdded -= ChangeValue;
                elements.OnElementCreated -= ChangeValue;
            }

            _changes = newInventory;
        }

        public override void Clear()
        {
            MClInventoryElements.Clear();
        }

        public override ModificationType Check_Changes()
        {
            var inventory = GetInventory();

            foreach (var item in inventory.items)
            {
                if (!_changes.items.ContainsKey(item.Key) || _changes.items[item.Key] != item.Value)
                    return ModificationType.EditData;
            }

            return ModificationType.None;
        }

        public override void Remove_Changes()
        {
            LoadInventoryItems(_changes);
        }
    }
}
