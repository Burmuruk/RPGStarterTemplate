using System;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public class ElementCreation : ElementCreationUI
    {
        private ElementType _type = ElementType.None;

        public override Enum Type { get => _type; set => _type = (ElementType)value; }

        public ElementCreation()
        {

        }

        public ElementCreation(VisualElement container, int idx) : base(container, idx)
        {
            EnumField.Init(ElementType.None);
        }

        public override void Initialize(VisualElement container, int idx)
        {
            base.Initialize(container, idx);

            EnumField.Init(ElementType.None);
        }
    }
}
