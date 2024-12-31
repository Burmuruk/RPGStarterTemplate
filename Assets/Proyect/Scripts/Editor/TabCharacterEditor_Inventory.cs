using Burmuruk.Tesis.Inventory;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public partial class TabCharacterEditor : BaseLevelEditor
    {
        const string infoEquipmentName = "EquipmentSettings";
        const string infoInventoryName = "InventorySettings";

        Button btnBackEquipmentSettings;
        Button btnBackInventorySettings;
        ComponentsListUI<ElementCreation> mclInventoryElements;
        ComponentsListUI<ElementCreation> mclEquipmentElements;
        ComponentsListUI<ElementComponent> characterComponents;
        ComponentsList<ElementCreation> creations;
        VisualElement infoBodyPlacement;
        EnumModifierUI emBodyPart;
        ElementType[] inventoryChoices = new ElementType[]
        {
            ElementType.None,
            ElementType.Item,
            ElementType.Consumable,
            ElementType.Weapon,
            ElementType.Armour,
            ElementType.Ability,
        };

        class EquipmentVisualizer : ScriptableObject
        {
            [SerializeField] public Tesis.Inventory.Equipment equipment;
        }

        private void Create_EquipmentSettings()
        {
            btnBackEquipmentSettings = infoContainers[INFO_EQUIPMENT_SETTINGS_NAME].Q<Button>();
            btnBackEquipmentSettings.clicked += () => ChangeTab(lastTab);
            emBodyPart = new EnumModifierUI(infoContainers[INFO_EQUIPMENT_SETTINGS_NAME].Q<VisualElement>(EnumModifierUI.ContainerName), Notify, EquipmentType.None);

            infoBodyPlacement = infoContainers[INFO_EQUIPMENT_SETTINGS_NAME].Q<VisualElement>("infoBodySplit");
            CreateSplitViewEquipment(infoBodyPlacement);

            var container = infoContainers[INFO_EQUIPMENT_SETTINGS_NAME].Q<VisualElement>(ComponentsList.CONTAINER_NAME);
            mclEquipmentElements = new (container, Notify);
        }

        private void CreateSplitViewEquipment(VisualElement container)
        {
            TwoPaneSplitView splitView = new TwoPaneSplitView();
            splitView.orientation = TwoPaneSplitViewOrientation.Horizontal;
            splitView.fixedPaneInitialDimension = 215;
            splitView.AddToClassList("SplitViewStyle");

            var bodVis = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Proyect/Game/UIToolkit/CharacterEditor/Elements/BodyVisualizer.uxml");
            var leftSide = bodVis.Instantiate();
            var objField = leftSide.Q<ObjectField>();
            var tree = leftSide.Q<TreeView>();

            var equipVis = ScriptableObject.CreateInstance<EquipmentVisualizer>();
            VisualElement rightSide = new InspectorElement(equipVis);

            objField.objectType = typeof(GameObject);
            objField.RegisterValueChangedCallback(evt => OnBodyPicked(evt.newValue, leftSide.Q<TreeView>(), equipVis));

            splitView.Insert(0, leftSide);
            splitView.Insert(1, rightSide);

            container.Add(splitView);
        }

        private void OnBodyPicked(UnityEngine.Object body, TreeView tree, EquipmentVisualizer rightSide)
        {
            var go = ((GameObject)body).transform;
            int startIdx = 0;
            tree.Clear();

            List<TreeViewItemData<string>> subItemData = GetChilds(go.transform, ref startIdx);

            tree.makeItem = () => new Label();
            tree.bindItem = (elem, idx) =>
            {
                elem.Q<Label>().text = subItemData[idx].data;
            };

            tree.SetRootItems(subItemData);
            tree.Rebuild();
            rightSide.equipment.Body = (GameObject)body;
        }

        private List<TreeViewItemData<string>> GetChilds(Transform transform, ref int idx)
        {
            var subItemData = new List<TreeViewItemData<string>>();

            for (; idx < transform.childCount; idx++)
            {
                if (transform.GetChild(idx).childCount > 0)
                {
                    int cur = idx++;
                    var childs = GetChilds(transform.GetChild(cur), ref idx);

                    subItemData.Add(new TreeViewItemData<string>(cur, transform.GetChild(cur).name, childs));
                }
                else
                    subItemData.Add(new TreeViewItemData<string>(idx, transform.GetChild(idx).name));
            }

            return subItemData;
        }

        private void Load_InventoryItemsInEquipment()
        {
            for (int i = 0; i < mclEquipmentElements.Components.Count; i++)
            {
                EnableContainer(mclEquipmentElements.Components[i].element, false);
            }

            int idx = 0;
            foreach (var component in mclInventoryElements.Components)
            {
                mclEquipmentElements.AddElement(component.NameButton.text);
                Setup_EquipmentElementButton(idx++);
            }
        }

        private void Create_InventorySettings()
        {
            btnBackInventorySettings = infoContainers[INFO_INVENTORY_SETTINGS_NAME].Q<Button>();
            btnBackInventorySettings.clicked += () => ChangeTab(lastTab);

            var container = infoContainers[INFO_INVENTORY_SETTINGS_NAME].Q<VisualElement>(ComponentsList.CONTAINER_NAME);
            mclInventoryElements = new(container, Notify);
            
            mclInventoryElements.DDFType.RegisterValueChangedCallback((evt) => OnValueChanged_EFInventoryType(evt));
            mclInventoryElements.DDFType.SetValueWithoutNotify("None");

            mclInventoryElements.DDFElement.RegisterValueChangedCallback((evt) => OnValueChanged_DDFInventoryElement(evt.newValue));
            mclInventoryElements.DDFElement.SetValueWithoutNotify("None");
            Populate_DDFsInventory();

            MultiColumnListView lstInventory = new MultiColumnListView();
        }

        private void OnValueChanged_EFInventoryType(ChangeEvent<string> evt)
        {
            mclInventoryElements.DDFElement.choices.Clear();
            var type = Enum.Parse<ElementType>(evt.newValue);
            mclInventoryElements.DDFElement.SetValueWithoutNotify("None");

            if (!charactersLists.elements.ContainsKey(type))
                return;

            foreach (var value in charactersLists.elements[type])
            {
                mclInventoryElements.DDFElement.choices.Add(value);
            }
        }

        private void OnValueChanged_DDFInventoryElement(string name)
        {
            if (name == "None") return;

            int? elementIdx = Check_HasInventoryComponent(name);

            if (elementIdx.HasValue)
            {
                mclInventoryElements.IncrementElement(elementIdx.Value);
                return;
            }

            mclInventoryElements.AddElement(name);
        }

        private void Populate_DDFsInventory()
        {
            mclInventoryElements.DDFType.choices.Clear();
            mclInventoryElements.DDFElement.choices.Clear();

            foreach (var type in inventoryChoices)
            {
                mclInventoryElements.DDFType.choices.Add(type.ToString());

                if (!charactersLists.creations.ContainsKey(type))
                    continue;

                foreach (var creation in charactersLists.creations[type])
                {
                    mclInventoryElements.DDFElement.choices.Add(creation.Key);
                }
            }
        }

        private int? Check_HasInventoryComponent(string name)
        {
            for (int i = 0; i < mclInventoryElements.Components.Count; i++)
            {
                if (!mclInventoryElements.Components[i].element.ClassListContains("Disable") && mclInventoryElements.Components[i].NameButton.text.Contains(name))
                    return i;
            }

            return null;
        }

        private void Setup_EquipmentElementButton(int componentIdx)
        {
            var type = (ElementType)mclEquipmentElements[componentIdx].Type;
            if (type == ElementType.Armour || type == ElementType.Weapon || type == ElementType.Ability)
            {
                mclEquipmentElements.Components[componentIdx].Toggle.SetEnabled(true);
            }
            else
            {
                mclEquipmentElements.Components[componentIdx].Toggle.SetEnabled(false);
            }

            if (!mclEquipmentElements.Components[componentIdx].Toggle.ClassListContains("Disable"))
                return;

            EnableContainer(mclEquipmentElements.Components[componentIdx].RemoveButton, false);
            EnableContainer(mclEquipmentElements.Components[componentIdx].IFAmount, false);
            EnableContainer(mclEquipmentElements.Components[componentIdx].Toggle, true);

            mclEquipmentElements.Components[componentIdx].Toggle.RegisterValueChangedCallback((evt) => OnValueChanged_TglEquipment(evt.newValue, componentIdx));
            mclEquipmentElements.Components[componentIdx].EnumField.Init(EquipmentType.None);
            mclEquipmentElements.Components[componentIdx].EnumField.RegisterValueChangedCallback((evt) => OnValueChanged_EFEquipment(evt.newValue, componentIdx));
        }

        private void OnValueChanged_EFEquipment(Enum newValue, int componentIdx)
        {

        }

        private void OnValueChanged_TglEquipment(bool newValue, int componentIdx)
        {
            EnableContainer(mclEquipmentElements.Components[componentIdx].EnumField, newValue);
        }

        private Inventory GetInventory()
        {
            var inventory = new Inventory();
            inventory.items = new();

            for (int i = 0; i < mclInventoryElements.Components.Count; i++)
            {
                if (mclInventoryElements[i].element.ClassListContains("Disable"))
                    continue;

                inventory.items.TryAdd((ElementType)mclInventoryElements[i].Type, mclInventoryElements.Amounts[i]);
            }

            return inventory;
        }

        private Equipment GetEquipment(in Inventory inventory)
        {
            var equipment = new Equipment(inventory);

            for (int i = 0; i < mclEquipmentElements.Components.Count; i++)
            {
                if (mclEquipmentElements[i].Toggle.value)
                {
                    var type = (ElementType)mclEquipmentElements[i].Type;

                    equipment.equipment.TryAdd(type, new EquipData()
                    {
                        type = type,
                        place = Enum.Parse<EquipmentType>(mclEquipmentElements[i].EnumField.text),
                        equipped = true,
                    });
                }
            }

            return equipment;
        }
    }
}
