using Burmuruk.Tesis.Editor.Utilities;
using Burmuruk.Tesis.Inventory;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class EquipmentSettings : SubWindow
    {
        const string INFO_EQUIPMENT_SETTINGS_NAME = "EquipmentSettings";
        Equipment _Changes = default;

        class EquipmentVisualizer : ScriptableObject
        {
            [SerializeField] public Tesis.Inventory.Equipment equipment;
        }

        public Button BTNBackEquipmentSettings { get; private set; }
        public ComponentsListUI<ElementCreation> MClEquipmentElements { get; private set; }
        public EnumModifierUI<EquipmentType> EMBodyPart { get; private set; }
        public VisualElement InfoBodyPlacement { get; private set; }

        public EquipmentSettings(VisualElement container)
        {
            _container = container;
            _instance = UtilitiesUI.CreateDefaultTab(INFO_EQUIPMENT_SETTINGS_NAME);
            _container.hierarchy.Add(_instance);
        }

        public override void Initialize(VisualElement container)
        {
            base.Initialize(container);

            BTNBackEquipmentSettings = container.Q<Button>();
            BTNBackEquipmentSettings.clicked += () => GoBack?.Invoke();

            EMBodyPart = new EnumModifierUI<EquipmentType>(_instance.Q<VisualElement>(EnumModifierUI<EquipmentType>.ContainerName));

            InfoBodyPlacement = _instance.Q<VisualElement>("infoBodySplit");
            CreateSplitViewEquipment(InfoBodyPlacement);

            var equipmentList = _instance.Q<VisualElement>(ComponentsList.CONTAINER_NAME);
            MClEquipmentElements = new(equipmentList);
            MClEquipmentElements.OnElementCreated += DisablePin;
        }

        private void DisablePin(ElementCreation element)
        {
            EnableContainer(element.element.Q<Button>("btnPin"), false);
        }

        private void Setup_EquipmentElementButton(int componentIdx)
        {
            var type = (ElementType)MClEquipmentElements[componentIdx].Type;
            if (type == ElementType.Armour || type == ElementType.Weapon || type == ElementType.Ability)
            {
                MClEquipmentElements.Components[componentIdx].Toggle.SetEnabled(true);
            }
            else
            {
                MClEquipmentElements.Components[componentIdx].Toggle.SetEnabled(false);
            }

            if (!MClEquipmentElements.Components[componentIdx].Toggle.ClassListContains("Disable"))
                return;

            EnableContainer(MClEquipmentElements.Components[componentIdx].RemoveButton, false);
            EnableContainer(MClEquipmentElements.Components[componentIdx].IFAmount, false);
            EnableContainer(MClEquipmentElements.Components[componentIdx].Toggle, true);

            MClEquipmentElements.Components[componentIdx].Toggle.RegisterValueChangedCallback((evt) => OnValueChanged_TglEquipment(evt.newValue, componentIdx));
            MClEquipmentElements.Components[componentIdx].EnumField.Init(EquipmentType.None);
            MClEquipmentElements.Components[componentIdx].EnumField.RegisterValueChangedCallback((evt) => OnValueChanged_EFEquipment(evt.newValue, componentIdx));
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
            for (int i = 0; i < MClEquipmentElements.Components.Count; i++)
            {
                EnableContainer(MClEquipmentElements.Components[i].element, false);
            }

            int idx = 0;
            foreach (var component in inventory.Components)
            {
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

        public Equipment GetEquipment(in Inventory inventory)
        {
            var equipment = new Equipment(inventory);

            for (int i = 0; i < MClEquipmentElements.Components.Count; i++)
            {
                if (MClEquipmentElements[i].Toggle.value)
                {
                    var type = (ElementType)MClEquipmentElements[i].Type;

                    equipment.equipment.TryAdd(type, new EquipData()
                    {
                        type = type,
                        place = Enum.Parse<EquipmentType>(MClEquipmentElements[i].EnumField.text),
                        equipped = true,
                    });
                }
            }

            return equipment;
        }

        public void LoadEquipment(in Equipment equipment)
        {
            MClEquipmentElements.RestartValues();

            if (equipment.equipment != null)
                foreach (var item in equipment.equipment)
                {
                    Action<ElementCreation> EditData = (e) =>
                    {
                        e.Toggle.value = item.Value.equipped;
                        e.EnumField.value = item.Value.place;
                    };
                    MClEquipmentElements.OnElementCreated += (e) => EditData(e);
                    MClEquipmentElements.OnElementAdded += (e) => EditData(e);

                    MClEquipmentElements.AddElement(item.Key.ToString());

                    MClEquipmentElements.OnElementCreated -= (e) => EditData(e);
                    MClEquipmentElements.OnElementAdded -= (e) => EditData(e);
                }
        }

        public override void Clear()
        {
            EMBodyPart.Clear();
        }

        public override ModificationTypes Check_Changes()
        {


            return ModificationTypes.None;
        }

        public override void Remove_Changes()
        {
            var newData = _Changes;
            LoadEquipment(newData);
        }
    }
}
