using Burmuruk.Tesis.Inventory;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class EquipmentSpawnsList : MyCustomList
    {
        Dictionary<string, (Transform transform, EquipmentType place)> _changes;
        LinkedList<ElementData> _elements = new();
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
                element.AddToClassList("LineElements");
                label.style.maxWidth = 20;
                label.text = name;

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
                    transform.SetValueWithoutNotify(values[0]);
                }
            }

            public void Clear()
            {
                transform = null;
                place = default;
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

            foreach (var item in _elements)
            {
                var transform = (item.transform.value as GameObject).transform;
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
                _elements.Last.Value.transform.value = values[0];
            }
        }

        private void ChangeAmount(KeyUpEvent evt)
        {
            if (evt.keyCode != KeyCode.Return && evt.keyCode != KeyCode.KeypadEnter) return;

            int amount = ((int)TxtCount.value) - _elements.Count;

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

        public void UpdateData(List<(Transform transform, EquipmentType place)> newData)
        {
            Clear();

            foreach (var item in newData)
            {
                Add();
                _elements.Last.Value.place.value = item.place;
                _elements.Last.Value.transform.value = item.transform;

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

            _elements.AddLast(data);
            _elementsContainer.Add(data.container);
            TxtCount.value = (uint)_elements.Count;
        }

        private void Remove()
        {
            if (_elements.Count <= 0) return;

            _disabledElements.AddLast(_elements.Last.Value);
            _elementsContainer.Remove(_elements.Last.Value.container);
            _elements.RemoveLast();

            _disabledElements.Last.Value.Clear();
            TxtCount.value = (uint)_elements.Count;
        }

        public override void Clear()
        {
            base.Clear();
            _changes.Clear();
            TxtCount.value = 0;

            while (_elements.Count > 0)
            {
                _disabledElements.AddLast(_elements.First.Value);
                _elements.RemoveFirst();

                _disabledElements.Last.Value.Clear();
            }
        }

        public override ModificationTypes Check_Changes()
        {
            if (_changes == null) return ModificationTypes.None;

            foreach (ElementData element in _elements)
            {
                if (_changes.ContainsKey(element.transform.value.name))
                {
                    if (_changes[element.transform.name].place != (EquipmentType)element.place.value)
                        return ModificationTypes.EditData;
                }
                else
                {
                    return ModificationTypes.EditData;
                }
            }

            return ModificationTypes.None;
        }
    }
}
