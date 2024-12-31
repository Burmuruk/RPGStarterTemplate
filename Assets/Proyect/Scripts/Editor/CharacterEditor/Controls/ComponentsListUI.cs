using System;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public class ComponentsListUI<T> : ComponentsList<T> where T : ElementCreationUI, new()
    {
        public DropdownField DDFType { get; private set; }
        public DropdownField DDFElement { get; private set; }

        public ComponentsListUI(VisualElement container, Action<string, BaseLevelEditor.BorderColour> notifyCallback) : base(container, notifyCallback)
        {
            DDFType = container.Q<DropdownField>("ddfType");
            DDFElement = container.Q<DropdownField>("ddfElement");
        }

        private void Initialize_DDAddComponent()
        {
            DDFElement.RegisterValueChangedCallback(AddComponent);
        }

        private void AddComponent(ChangeEvent<string> evt)
        {
            if (Check_HasCharacterComponent(evt.newValue)) return;
            AddElement(evt.newValue);

            DDFElement.SetValueWithoutNotify("None");
        }

        protected override T CreateNewComponent(string value, out int idx)
        {
            var component = base.CreateNewComponent(value, out idx);
            int newIdx = idx;

            //component.NameButton.clicked += () => OpenComponentSettings(idx);
            component.NameButton.clicked += () => bindElementBtn(newIdx);
            component.RemoveButton.clicked += () => RemoveComponent(newIdx);
            component.IFAmount.RegisterValueChangedCallback((evt) => UpdateTxtAmount(newIdx, evt.newValue));

            return component;
        }

        private void UpdateTxtAmount(int idx, int newValue)
        {
            if (newValue < 0)
            {
                notifyCallback("Negative values are not allowed", BaseLevelEditor.BorderColour.Error);
                return;
            }

            var amount = newValue - Amounts[idx];

            if (amount < 0)
            {
                ChangeAmount(idx, 0);
            }
            else
            {
                ChangeAmount(idx, amount);
            }
        }
    }
}
