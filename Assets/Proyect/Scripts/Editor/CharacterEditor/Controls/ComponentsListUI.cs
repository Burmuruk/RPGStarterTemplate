using Burmuruk.Tesis.Editor.Utilities;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class ComponentsListUI<T> : ComponentsList<T> where T : ElementCreationUI, new()
    {
        public DropdownField DDFType { get; private set; }
        public DropdownField DDFElement { get; private set; }

        public ComponentsListUI(VisualElement container) : base(container)
        {
            DDFType = container.Q<DropdownField>("ddfType");
            DDFElement = container.Q<DropdownField>("ddfElement");

            VisualElement element = new VisualElement();
            element.style.height = 50;
            element.style.width = 30;
            container.hierarchy.Add(element);
        }

        public void AddComponent(ChangeEvent<string> evt)
        {
            if (Check_HasCharacterComponent(evt.newValue))
            {
                DDFElement.SetValueWithoutNotify("None");
                return;
            }

            if (DDFType == null)
                AddElement(evt.newValue);
            else
                AddElement(evt.newValue, DDFType.value);

            DDFElement.SetValueWithoutNotify("None");
        }

        protected override T CreateNewComponent(string value, string type, out int idx)
        {
            var component = base.CreateNewComponent(value, type, out idx);
            int newIdx = idx;

            component.IFAmount.RegisterValueChangedCallback((evt) => UpdateTxtAmount(newIdx, evt.newValue));

            return component;
        }

        private void UpdateTxtAmount(int idx, int newValue)
        {
            ChangeAmount(idx, newValue);
        }

        public override void Clear()
        {
            base.Clear();

            DDFType?.SetValueWithoutNotify("None");
            DDFElement?.SetValueWithoutNotify("None");
        }
    }
}
