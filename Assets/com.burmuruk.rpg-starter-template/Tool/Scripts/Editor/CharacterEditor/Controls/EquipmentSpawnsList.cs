using Burmuruk.Tesis.Inventory;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class EquipmentSpawnsList : MyCustomList, IChangesObserver
    {
        Dictionary<string, (Transform transform, EquipmentType place)> _changes;
        LinkedList<ElementData> _enabledElements = new();
        LinkedList<ElementData> _disabledElements = new();

        private struct ElementData
        {
            public VisualElement container;
            public ObjectField transform;
            public EnumField place;

            public ElementData(VisualElement container)
            {
                this.container = container;
                
                transform = new ObjectField("");
                place = new EnumField("", default(EquipmentType));
                
                var row1 = InsertInRow(transform, "Spawn point");
                var row2 = InsertInRow(place, "Place");
                row2.style.marginBottom = 6;
                container.Add(row1);
                container.Add(row2);

                Setup_Transform();
            }

            private VisualElement InsertInRow(VisualElement element, string name)
            {
                var row = Get_Row();
                Label label = new Label();
                label.AddToClassList("ElementTag");
                label.style.flexShrink = 2;
                label.style.flexGrow = 0;
                label.style.maxWidth = 20;
                label.style.minWidth = new StyleLength(StyleKeyword.None);
                label.style.maxWidth = new StyleLength(StyleKeyword.None);
                label.style.paddingRight = 5;
                label.text = name;
                element.AddToClassList("LineElements");
                element.style.flexShrink = 1;
                element.style.flexGrow = 1;
                element.style.flexBasis = new Length(50, LengthUnit.Percent);

                row.Add(label);
                row.Add(element);
                return row;
            }

            private VisualElement Get_Row()
            {
                VisualElement row = new VisualElement();

                row.AddToClassList("LineContainer");
                return row;
            }

            private void Setup_Transform()
            {
                transform.objectType = typeof(GameObject);

                transform.RegisterCallback<DragEnterEvent>(evt =>
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                });

                transform.RegisterCallback<DragPerformEvent>(OnBoneDropped);
            }

            private void OnBoneDropped(DragPerformEvent evt)
            {
                var values = DragAndDrop.GetGenericData("DraggedNode") as UnityEngine.Object[];

                if (values != null && values.Length > 0)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        Debug.Log($"Spawn point: {values[i].name}"); 
                    }
                    transform.SetValueWithoutNotify(values[0]);
                }
            }

            public void Clear()
            {
                transform.value = null;
                place.value = default(EquipmentType);
            }
        }

        public EquipmentSpawnsList(VisualElement container) : base(container)
        {
            BtnAdd.clicked += Add;
            BtnRemove.clicked += Remove;

            TxtCount.RegisterCallback<KeyUpEvent>(ChangeAmount);
            TxtCount.RegisterCallback<DragPerformEvent>(OnBoneDropped);
        }

        public List<(Transform transform, EquipmentType type)> GetInfo()
        {
            var spawnPoints = new List<(Transform transform, EquipmentType type)>();

            foreach (var item in _enabledElements)
            {
                Transform transform = null;
                if ((item.transform.value as GameObject) != null)
                    transform = (item.transform.value as GameObject).transform;
                else
                    transform = (item.transform.value as Transform).transform;
                spawnPoints.Add((transform, (EquipmentType)item.place.value));
            }

            return spawnPoints;
        }

        private void OnBoneDropped(DragPerformEvent evt)
        {
            var values = DragAndDrop.GetGenericData("DraggedNode") as UnityEngine.Object[];

            if (values != null && values.Length > 0)
            {
                Add();
                _enabledElements.Last.Value.transform.value = values[0];
            }
        }

        private void ChangeAmount(KeyUpEvent evt)
        {
            if (evt.keyCode != KeyCode.Return && evt.keyCode != KeyCode.KeypadEnter) return;

            int amount = ((int)TxtCount.value) - _enabledElements.Count;

            if (amount == 0)
            {
                Clear();
            }
            else if (amount > 0)
            {
                while (amount > 0)
                {
                    Add();
                    --amount;
                }
            }
            else
            {
                while (amount < 0)
                {
                    Remove();
                    ++amount;
                }
            }
        }

        protected override void SetupFoldOut()
        {
            base.SetupFoldOut();

            Foldout.text = "Spawn points";
        }

        public void LoadInfo(List<(Transform transform, EquipmentType place)> newData)
        {
            Clear();
            _changes = new();
            Debug.Log("hi");
            foreach (var item in newData)
            {
                Add();
                _enabledElements.Last.Value.place.value = item.place;
                _enabledElements.Last.Value.transform.value = item.transform;

                _changes.Add(item.transform.name, item);
            }
        }

        public void Add()
        {
            ElementData data;

            if (_disabledElements.Count > 0)
            {
                data = _disabledElements.First.Value;
                _disabledElements.RemoveFirst();
            }
            else
            {
                data = new ElementData(new VisualElement());
            }

            _enabledElements.AddLast(data);
            _elementsContainer.Add(data.container);
            TxtCount.value = (uint)_enabledElements.Count;
        }

        private void Remove()
        {
            if (_enabledElements.Count <= 0) return;

            _disabledElements.AddLast(_enabledElements.Last.Value);
            _elementsContainer.Remove(_enabledElements.Last.Value.container);
            _enabledElements.RemoveLast();

            _disabledElements.Last.Value.Clear();
            TxtCount.value = (uint)_enabledElements.Count;
        }

        public override void Clear()
        {
            base.Clear();
            _changes = null;
            TxtCount.value = 0;

            while (_enabledElements.Count > 0)
            {
                _disabledElements.AddLast(_enabledElements.First.Value);
                _enabledElements.RemoveFirst();

                _disabledElements.Last.Value.Clear();
            }
        }

        public override ModificationTypes Check_Changes()
        {
            if (_changes == null) return ModificationTypes.None;

            foreach (ElementData element in _enabledElements)
            {
                if (_changes.ContainsKey(element.transform.value.name))
                {
                    if (_changes[element.transform.value.name].place != (EquipmentType)element.place.value)
                        return ModificationTypes.EditData;
                }
                else
                {
                    return ModificationTypes.EditData;
                }
            }

            return ModificationTypes.None;
        }

        public void Load_Changes()
        {
            throw new System.NotImplementedException();
        }

        public void Remove_Changes()
        {
            throw new System.NotImplementedException();
        }

        public bool VerifyData()
        {
            bool result = true;

            foreach (var element in _enabledElements)
            {
                bool value = false;
                result &= value = element.transform.value != null;
                Highlight(element.transform, !value, BorderColour.Error);

                var place = (EquipmentType)element.place.value;
                result &= value = place != EquipmentType.None && place != EquipmentType.Body;
                Highlight(element.place, !value, BorderColour.Error);
            }

            return result;
        }
    }
}
