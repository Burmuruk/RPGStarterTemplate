using Burmuruk.Tesis.Inventory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Rendering.FilterWindow;

namespace Burmuruk.Tesis.Editor
{
	public partial class TabCharacterEditor : BaseLevelEditor
	{
        const string infoEquipmentName = "EquipmentSettings";
        const string infoInventoryName = "InventorySettings";

        Button btnBackEquipmentSettings;
        Button btnBackInventorySettings;
        MyComponentsList mclInventoryElements;
        MyComponentsList mclEquipmentElements;
        MyComponentsList curElementList;
        VisualElement infoBodyPlacement;
        EnumModifier emBodyPart;
        ElementType[] inventoryChoices = new ElementType[]
        {
            ElementType.None,
            ElementType.Item,
            ElementType.Consumable,
            ElementType.Weapon,
            ElementType.Armor,
            ElementType.Hability,
        };

        class EquipmentVisualizer : ScriptableObject
        {
            [SerializeField] public Tesis.Inventory.Equipment equipment;
        }

        struct MyComponentsList
        {
            public const string containerName = "infoComponents";
            List<int> amounts;

            public VisualElement container;
            public List<MyListElement> Components { get; private set; }
            public VisualElement infoContainer;
            public DropdownField ddfType;
            public DropdownField ddfElement;

            public MyListElement this[int index]
            {
                get => Components[index];
                set => Components[index] = value;
            }

            public MyComponentsList(VisualElement container)
            {
                amounts = new();
                this.container = container;
                Components = new();
                infoContainer = container.Q<VisualElement>("componentsConatiner");
                ddfType = container.Q<DropdownField>("ddfType");
                ddfElement = container.Q<DropdownField>("ddfElement");
            }

            public void IncrementElement(int idx, bool shouldIncrement = true, int value = 1)
            {
                amounts[idx] += shouldIncrement ? value : -value;
                Components[idx].IFAmount.value = amounts[idx];
            }

            public bool ChangeAmount(int idx, int amount)
            {
                if (amount < 0)
                {
                    Components[idx].IFAmount.value = amounts[idx];
                    return false;
                }

                amounts[idx] = amount;
                Components[idx].IFAmount.value = amount;

                return true;
            }

            public void AddElement(MyListElement element)
            {
                Components.Add(element);
                element.IFAmount.RemoveFromClassList("Disable");
                element.IFAmount.value = 1;
                amounts.Add(1);
            }
        }

        struct MyListElement
        {
            public int idx;

            public VisualElement element;
            public Button NameButton { get; private set; }
            public Button RemoveButton { get; private set; }
            public Toggle Toggle { get; }
            public IntegerField IFAmount { get; private set; }
            public EnumField EnumField { get; private set; }
            public ElementType Type { get; set; }

            public MyListElement(VisualElement container, int idx)
            {
                this.idx = idx;
                element = container;
                NameButton = container.Q<Button>("btnEditComponent");
                RemoveButton = container.Q<Button>("btnRemove");
                Toggle = container.Q<Toggle>();
                IFAmount = container.Q<IntegerField>("txtAmount");
                EnumField = container.Q<EnumField>();
                Type = ElementType.None;

                Toggle.AddToClassList("Disable");
            }
        }

        private void Create_EquipmentSettings()
        {
            btnBackEquipmentSettings = infoContainers[infoEquipmentSettingsName].Q<Button>();
            btnBackEquipmentSettings.clicked += () => ChangeTab(lastTab);
            emBodyPart = new EnumModifier(infoContainers[infoEquipmentSettingsName].Q<VisualElement>(EnumModifier.ContainerName));
            emBodyPart.EnumField.Init(EquipmentType.None);

            infoBodyPlacement = infoContainers[infoEquipmentSettingsName].Q<VisualElement>("infoBodySplit");
            CreateSplitViewEquipment(infoBodyPlacement);

            mclEquipmentElements = new(infoContainers[infoEquipmentSettingsName].Q<VisualElement>(MyComponentsList.containerName));
        }

        private void CreateSplitViewEquipment(VisualElement container)
        {
            TwoPaneSplitView splitView = new TwoPaneSplitView();
            splitView.orientation = TwoPaneSplitViewOrientation.Horizontal;
            splitView.fixedPaneInitialDimension = 215;
            splitView.AddToClassList("SplitViewStyle");

            var bodVis = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Proyect/Game/UIToolkit/BodyVisualizer.uxml");
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

            List<TreeViewItemData<string>>  subItemData = GetChilds(go.transform, ref startIdx);

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
            for (int i = 0; i < curElementList.Components.Count; i++)
            {
                EnableContainer(curElementList.Components[i].element, false); 
            }

            int idx = 0;
            foreach (var component in mclInventoryElements.Components)
            {
                Add_InventoryElement(component.NameButton.text, component.Type);
                Setup_EquipmentElementButton(idx++);
            }
        }

        private void Create_InventorySettings()
        {
            btnBackInventorySettings = infoContainers[infoInventorySettingsName].Q<Button>();
            btnBackInventorySettings.clicked += () => ChangeTab(lastTab);

            mclInventoryElements = new(infoContainers[infoInventorySettingsName].Q<VisualElement>(MyComponentsList.containerName));
            mclInventoryElements.ddfType.RegisterValueChangedCallback((evt) => OnValueChanged_EFInventoryType(evt));
            mclInventoryElements.ddfType.choices.Clear();
            mclInventoryElements.ddfType.SetValueWithoutNotify("None");

            mclInventoryElements.ddfElement.RegisterValueChangedCallback((evt) => OnValueChanged_EFInventoryElement(evt.newValue));
            mclInventoryElements.ddfElement.choices.Clear();
            mclInventoryElements.ddfElement.SetValueWithoutNotify("None");

            foreach (var item in inventoryChoices)
            {
                mclInventoryElements.ddfType.choices.Add(item.ToString());
            }

            MultiColumnListView lstInventory = new MultiColumnListView();
        }

        private void OnValueChanged_EFInventoryType(ChangeEvent<string> evt)
        {
            curElementList.ddfElement.choices.Clear();
            var type = Enum.Parse<ElementType>(evt.newValue);
            mclInventoryElements.ddfElement.SetValueWithoutNotify("None");

            if (!charactersLists.elements.ContainsKey(type))
                return;

            foreach (var value in charactersLists.elements[type])
            {
                curElementList.ddfElement.choices.Add(value);
            }
        }

        private void Populate_DDFElements(MyComponentsList mcl, ElementType type)
        {
            mcl.ddfElement.Clear();


            foreach (var value in charactersLists.elements[type])
            {
                mcl.ddfElement.choices.Add(value); 
            }
        }

        private void OnValueChanged_EFInventoryElement(string name)
        {
            if (name == "None") return;

            int? elementIdx = Check_HasInventoryComponent(name);

            if (elementIdx.HasValue)
            {
                curElementList.IncrementElement(elementIdx.Value);
                return;
            }
            
            var type = Enum.Parse<ElementType>(curElementList.ddfType.value);
            Add_InventoryElement(name, type);
            //Setup_InventoryElementButton(type, elementIdx.Value);
        }

        private int? Check_HasInventoryComponent(string name)
        {
            for (int i = 0; i < curElementList.Components.Count; i++)
            {
                if (!curElementList.Components[i].element.ClassListContains("Disable") && curElementList.Components[i].NameButton.text.Contains(name))
                    return i;
            }

            return null;
        }

        private void Add_InventoryElement(string value, ElementType type)
        {
            int componentIdx = -1;

            for (int i = 0; i < curElementList.Components.Count; i++)
            {
                if (curElementList.Components[i].element.ClassListContains("Disable"))
                {
                    componentIdx = i;
                    EnableContainer(curElementList.Components[i].element, true);
                    break;
                }
            }

            if (componentIdx == -1)
                CreateNewInventoryComponent(value, out componentIdx);

            var elementData = curElementList.Components[componentIdx];
            elementData.Type = type;
            curElementList.Components[componentIdx] = elementData;
            curElementList.Components[componentIdx].NameButton.text = value;

            //characterData.Components ??= new();
            //characterData.Components.Add(type, null);

            //Setup_InventoryElementButton(type, componentIdx);
        }

        private MyListElement CreateNewInventoryComponent(string value, out int idx)
        {
            int newIdx = idx = curElementList.Components.Count;

            VisualTreeAsset element = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Proyect/Game/UIToolkit/ElementComponent.uxml");
            var component = new MyListElement(element.Instantiate(), newIdx);

            //component.BtnEditComponent.clicked += () => OpenComponentSettings(newIdx);
            component.RemoveButton.clicked += () => Remove_ListElement(newIdx);
            component.IFAmount.RegisterValueChangedCallback((evt) => UpdateTxtAmount(newIdx, evt.newValue));

            curElementList.AddElement(component);
            curElementList.infoContainer.Add(curElementList.Components[idx].element);
            return component;
        }

        private void UpdateTxtAmount(int idx, int value)
        {
            if (!curElementList.ChangeAmount(idx, value))
                Notify ("Negative values are not allowed", BorderColour.Error);
        }

        private void Setup_InventoryElementButton(ElementType type, int componentIdx)
        {
            //switch (type)
            //{
            //    case ElementType.Inventory:
            //    case ElementType.Equipment:
            //    case ElementType.Health:
            //        Components[componentIdx].BtnEditComponent.AddToClassList("ClickableBtn");
            //        Components[componentIdx].BtnEditComponent.style.backgroundColor = new Color(0.4627451f, 0.4627451f, 4627451f);
            //        break;

            //    default:
            //        if (Components[componentIdx].BtnEditComponent.ClassListContains("ClickableBtn"))
            //            Components[componentIdx].BtnEditComponent.RemoveFromClassList("ClickableBtn");
            //        Components[componentIdx].BtnEditComponent.style.backgroundColor = new Color(0.1647059f, 0.1647059f, 0.1647059f);
            //        break;
            //}
        }

        private void Setup_EquipmentElementButton(int componentIdx)
        {
            var type = curElementList[componentIdx].Type;
            if (type == ElementType.Armor || type == ElementType.Weapon || type == ElementType.Hability)
            {
                curElementList.Components[componentIdx].Toggle.SetEnabled(true);
            }
            else
            {
                curElementList.Components[componentIdx].Toggle.SetEnabled(false);
            }


            if (!curElementList.Components[componentIdx].Toggle.ClassListContains("Disable"))
                return;
                
            EnableContainer(curElementList.Components[componentIdx].RemoveButton, false);
            EnableContainer(curElementList.Components[componentIdx].IFAmount, false);
            EnableContainer(curElementList.Components[componentIdx].Toggle, true);

            curElementList.Components[componentIdx].Toggle.RegisterValueChangedCallback((evt) => OnValueChanged_TglEquipment(evt.newValue, componentIdx));
            curElementList.Components[componentIdx].EnumField.Init(EquipmentType.None);
            curElementList.Components[componentIdx].EnumField.RegisterValueChangedCallback((evt) => OnValueChanged_EFEquipment(evt.newValue, componentIdx));
        }

        private void OnValueChanged_EFEquipment(Enum newValue, int componentIdx)
        {
            
        }

        private void OnValueChanged_TglEquipment(bool newValue, int componentIdx)
        {
            EnableContainer(curElementList.Components[componentIdx].EnumField, newValue);
        }

        private void Remove_ListElement(int idx)
        {
            curElementList.ChangeAmount(idx, 0);
            EnableContainer(curElementList.Components[idx].element, false);
        }
    }
}
