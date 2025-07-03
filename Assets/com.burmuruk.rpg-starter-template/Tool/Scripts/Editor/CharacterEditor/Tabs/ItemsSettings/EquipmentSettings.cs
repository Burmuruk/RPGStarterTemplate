using Burmuruk.Tesis.Editor.Utilities;
using Burmuruk.Tesis.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class EquipmentSettings : SubWindow, IUIListContainer<BaseCreationInfo>
    {
        const string INFO_EQUIPMENT_SETTINGS_NAME = "EquipmentSettings";
        Equipment? _changes = null;
        Inventory _inventory;
        Queue<string> _itemsIds = new();

        public Button BTNBackEquipmentSettings { get; private set; }
        public ComponentsListUI<ElementCreation> MClEquipmentElements { get; private set; }
        public EnumModifierUI<EquipmentType> EMBodyPart { get; private set; }
        public VisualElement InfoBodyPlacement { get; private set; }
        public ObjectField OFBody { get; private set; }
        public TreeView TVBodyParts { get; private set; }
        public EquipmentSpawnsList UIParts { get; private set; }

        public override void Initialize(VisualElement container)
        {
            _instance = UtilitiesUI.CreateDefaultTab(INFO_EQUIPMENT_SETTINGS_NAME);
            container.hierarchy.Add(_instance);
            base.Initialize(_instance);

            BTNBackEquipmentSettings = _container.Q<Button>();
            BTNBackEquipmentSettings.clicked += () => GoBack?.Invoke();

            EMBodyPart = new EnumModifierUI<EquipmentType>(_instance.Q<VisualElement>(EnumModifierUI<EquipmentType>.ContainerName));
            EMBodyPart.Name.text = "Body Part";

            InfoBodyPlacement = _instance.Q<VisualElement>("infoBodySplit");
            CreateSplitViewEquipment(InfoBodyPlacement);

            var equipmentList = _instance.Q<VisualElement>(ComponentsList.CONTAINER_NAME);
            MClEquipmentElements = new(equipmentList);
            MClEquipmentElements.OnElementCreated += DisablePin;
            MClEquipmentElements.AddElementExtraData += Set_Id;
            RegisterToChanges();
        }

        private void Set_Id(ElementCreation creation)
        {
            if (_itemsIds.Count <= 0) return;

            creation.Id = _itemsIds.Dequeue();
        }

        #region Changes traker
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
        }

        public virtual void RenameCreation(in BaseCreationInfo newValue)
        {
            foreach (var component in MClEquipmentElements.Components)
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
            for (int i = 0; i < MClEquipmentElements.Components.Count; i++)
            {
                if (MClEquipmentElements[i].Id == newValue.Id)
                {
                    MClEquipmentElements.RemoveComponent(i);
                    return;
                }
            }
        }
        #endregion

        private void DisablePin(ElementCreation element)
        {
            EnableContainer(element.element.Q<Button>("btnPin"), false);
        }

        private void Setup_EquipmentElementButton(int componentIdx)
        {
            var type = (ElementType)MClEquipmentElements[componentIdx].Type;

            EnableContainer(MClEquipmentElements[componentIdx].RemoveButton, false);

            if (type == ElementType.Armour || type == ElementType.Weapon || type == ElementType.Ability)
            {
                MClEquipmentElements[componentIdx].Toggle.SetEnabled(true);
            }
            else
            {
                MClEquipmentElements[componentIdx].Toggle.SetEnabled(false);
                return;
            }

            //EnableContainer(MClEquipmentElements[componentIdx].IFAmount, false);
            EnableContainer(MClEquipmentElements[componentIdx].Toggle, true);

            MClEquipmentElements[componentIdx].Toggle.RegisterValueChangedCallback((evt) => OnValueChanged_TglEquipment(evt.newValue, componentIdx));
            MClEquipmentElements[componentIdx].EnumField.Init(EquipmentType.None);
            MClEquipmentElements[componentIdx].EnumField.RegisterValueChangedCallback((evt) => OnValueChanged_EFEquipment(evt.newValue, componentIdx));

            var item = ItemDataConverter.GetItem(type, MClEquipmentElements[componentIdx].Id);

            if (item as EquipeableItem is var equipable && equipable != null)
            {
                MClEquipmentElements[componentIdx].EnumField.SetValueWithoutNotify((EquipmentType)equipable.GetEquipLocation());
            }
        }

        private void OnValueChanged_EFEquipment(Enum newValue, int componentIdx)
        {

        }

        private void OnValueChanged_TglEquipment(bool newValue, int componentIdx)
        {
            EnableContainer(MClEquipmentElements.Components[componentIdx].EnumField, newValue);
        }

        public void Load_EquipmentFromList(ComponentsListUI<ElementCreation> inventory)
        {
            MClEquipmentElements.Components.ForEach(c => EnableContainer(c.element, false));

            int idx = 0;
            foreach (var component in inventory.Components)
            {
                _itemsIds.Enqueue(component.Id);
                MClEquipmentElements.AddElement(component.NameButton.text, component.Type.ToString());
                Setup_EquipmentElementButton(idx++);
            }
        }

        private void CreateSplitViewEquipment(VisualElement container)
        {
            TwoPaneSplitView splitView = new TwoPaneSplitView();
            splitView.orientation = TwoPaneSplitViewOrientation.Horizontal;
            splitView.fixedPaneInitialDimension = 215;
            splitView.AddToClassList("SplitViewStyle");

            var bodVis = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/com.burmuruk.rpg-starter-template/Tool/UIToolkit/CharacterEditor/Elements/BodyVisualizer.uxml");
            var leftSide = bodVis.Instantiate();
            var spawnFile = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/com.burmuruk.rpg-starter-template/Tool/UIToolkit/CharacterEditor/Elements/BodySpawnPoint.uxml");
            UIParts = new EquipmentSpawnsList(spawnFile.Instantiate());
            OFBody = leftSide.Q<ObjectField>();
            TVBodyParts = leftSide.Q<TreeView>();
            Setup_LeftSide(leftSide);
            Setup_TreeView();

            splitView.Insert(0, leftSide);
            splitView.Insert(1, UIParts.Container);
            container.Add(splitView);
        }

        private void Setup_LeftSide(VisualElement side)
        {
            OFBody.objectType = typeof(GameObject);
            OFBody.RegisterValueChangedCallback(ShowBodyTree);

            var scroll = side.Q<ScrollView>();
            scroll.RegisterCallback<WheelEvent>(evt => evt.StopPropagation());
        }

        //private void StopScroll(WheelEvent evt, ScrollView scroll)
        //{
        //    //bool scrollingDown = evt.delta.y > 0;
        //    //float scrollOffset = scroll.scrollOffset.y;
        //    //float contentHeight = scroll.contentContainer.layout.height;
        //    //float viewHeight = scroll.layout.height;

        //    //bool canScrollDown = scrollOffset + viewHeight < contentHeight;
        //    //bool canScrollUp = scrollOffset > 0;

        //    //if ((scrollingDown && canScrollDown) || (!scrollingDown && canScrollUp))
        //    //{
        //    //    // Si B aún puede desplazarse, evitamos que el scroll se propague al padre (A)
        //    //    evt.StopPropagation();
        //    //}
        //}

        private void Setup_TreeView()
        {
            TVBodyParts.SetEnabled(false);
            TVBodyParts.makeItem = () => new Label();
            
            TVBodyParts.bindItem = (element, i) =>
            {
                var data = TVBodyParts.GetItemDataForIndex<TransformNode>(i);
                var label = element as Label;
                label.text = data.name;
                label.userData = data;

                label.UnregisterCallback<PointerDownEvent>(OnPointerDown);
                label.RegisterCallback<PointerDownEvent>(OnPointerDown);
            };
            TVBodyParts.fixedItemHeight = 16;
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            var label = evt.target as Label;
            if (label == null || evt.button != 0) return;

            var data = label.userData as TransformNode;
            var go = data.transform?.gameObject;

            if (go == null || DragAndDrop.objectReferences.Length > 0) return;
            
            evt.StopPropagation();
                    
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = new UnityEngine.Object[] { go };
            DragAndDrop.StartDrag($"Dragging {go.name}");
        }

        private void ShowBodyTree(ChangeEvent<UnityEngine.Object> evt)
        {
            GameObject selected = evt.newValue as GameObject;
            if (selected == null) return;

            int idCounter = 0;
            var rootNode = BuildTree(selected.transform, ref idCounter);
            var rootTreeItem = BuildTreeItem(rootNode);

            if (rootTreeItem.children.Count() <= 0)
            {
                TVBodyParts.Clear();
                TVBodyParts.SetEnabled(false);
                return;
            }

            var rootItems = new List<TreeViewItemData<TransformNode>> { rootTreeItem };
            TVBodyParts.SetRootItems(rootItems);
            TVBodyParts.Rebuild();
            TVBodyParts.SetEnabled(true);

            EditorApplication.delayCall += () => CollapseAll(rootItems);
        }

        void CollapseAll(IEnumerable<TreeViewItemData<TransformNode>> items)
        {
            foreach (var item in items)
            {
                TVBodyParts.CollapseItem(item.id);
                if (item.children != null && item.children.Count() > 0)
                {
                    CollapseAll(item.children);
                }
            }
        }

        void FlattenTree(TransformNode node, List<TransformNode> list)
        {
            list.Add(node);
            foreach (var child in node.children)
                FlattenTree(child, list);
        }

        TreeViewItemData<TransformNode> BuildTreeItem(TransformNode node)
        {
            var children = node.children.Select(child => BuildTreeItem(child)).ToList();
            return new TreeViewItemData<TransformNode>(node.id, node, children);
        }

        private TransformNode BuildTree(Transform transform, ref int idCounter)
        {
            var node = new TransformNode
            {
                id = idCounter++,
                name = transform.name,
                transform = transform,
            };

            for (int i = 0; i < transform.childCount; i++)
            {
                node.children.Add(BuildTree(transform.GetChild(i), ref idCounter));
            }

            return node;
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

        public Equipment GetEquipment(in Inventory inventory)
        {
            var equipment = new Equipment(inventory)
            {
                model = OFBody.value as GameObject,
                spawnPoints = UIParts.GetInfo(),
            };

            for (int i = 0; i < MClEquipmentElements.Components.Count; i++)
            {
                if (MClEquipmentElements[i].Toggle.value)
                {
                    EquipmentType place = EquipmentType.None;
                    bool equipped = false;

                    if (!IsDisabled(MClEquipmentElements[i].EnumField))
                    {
                        place = Enum.Parse<EquipmentType>(MClEquipmentElements[i].EnumField.text);
                        equipped = MClEquipmentElements[i].Toggle.value;
                    }

                    equipment.equipment.TryAdd(MClEquipmentElements[i].Id, new EquipData()
                    {
                        type = (ElementType)MClEquipmentElements[i].Type,
                        place = place,
                        equipped = equipped,
                    });
                }
            }

            return equipment;
        }

        public void LoadEquipment(in Equipment equipment)
        {
            MClEquipmentElements.RestartValues();

            if (equipment.equipment == null) return;

            _changes = equipment;

            foreach (var item in equipment.equipment)
            {
                Action<ElementCreation> EditData = (e) =>
                {
                    e.Toggle.value = item.Value.equipped;
                    e.EnumField.value = item.Value.place;
                };
                if (!SavingSystem.Data.TryGetCreation(item.Key, out var data, out var type))
                    continue;

                MClEquipmentElements.OnElementCreated += (e) => EditData(e);
                MClEquipmentElements.OnElementAdded += (e) => EditData(e);

                MClEquipmentElements.AddElement(data.Name, type.ToString());

                MClEquipmentElements.OnElementCreated -= (e) => EditData(e);
                MClEquipmentElements.OnElementAdded -= (e) => EditData(e);
            }

            UIParts.LoadInfo(equipment.spawnPoints);
            OFBody.value = equipment.model;
        }

        public override void Clear()
        {
            OFBody.value = null;
            UIParts.Clear();
            EMBodyPart.Clear();
            TVBodyParts.Clear();

            foreach (var item in MClEquipmentElements.Components)
            {
                if (!IsDisabled(item.Toggle))
                {
                    item.Toggle.value = false;
                    item.EnumField.value = EquipmentType.None;
                }
            }

            _changes = null;
        }

        public override void Remove_Changes()
        {
            _changes = null;
        }

        public override bool VerifyData()
        {
            bool result = OFBody.value != null;
            Highlight(OFBody, !result, BorderColour.Error);

            result &= UIParts.VerifyData();

            return result;
        }

        public override ModificationTypes Check_Changes()
        {
            CurModificationType = ModificationTypes.None;

            if (!_changes.HasValue) return ModificationTypes.None;

            if (OFBody.value != _changes.Value.model) 
                return ModificationTypes.EditData;

            CurModificationType = UIParts.Check_Changes();

            foreach (var item in _changes.Value.equipment)
            {
                foreach (var element in MClEquipmentElements.Components)
                {
                    if (element.Id == item.Key)
                    {
                        if ((ElementType)element.Type != item.Value.type ||
                            element.Toggle.value != item.Value.equipped)
                        {
                            CurModificationType = ModificationTypes.EditData;
                            break;
                        }
                    }
                    else
                    {
                        CurModificationType = ModificationTypes.EditData;
                        break;
                    }
                }
            }

            return ModificationTypes.None;
        }

        public override void Load_Changes()
        {
            var newData = _changes.Value;
            LoadEquipment(newData);
        }
    }

    public class TransformNode
    {
        public int id;
        public string name;
        public List<TransformNode> children = new();
        public Transform transform;
    }
}
