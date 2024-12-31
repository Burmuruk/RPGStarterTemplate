using System;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public abstract class ElementCreationUI
    {
        public int _idx;

        public VisualElement element;
        public Button NameButton { get; private set; }
        public Button RemoveButton { get; private set; }
        public Toggle Toggle { get; private set; }
        public IntegerField IFAmount { get; private set; }
        public EnumField EnumField { get; private set; }
        public abstract Enum Type { get; set; }

        public ElementCreationUI()
        {

        }

        public ElementCreationUI(VisualElement container, int idx)
        {
            Initialize(container, idx);
        }

        public virtual void Initialize(VisualElement container, int idx)
        {
            this._idx = idx;
            element = container;
            NameButton = container.Q<Button>("btnEditComponent");
            RemoveButton = container.Q<Button>("btnRemove");
            Toggle = container.Q<Toggle>();
            IFAmount = container.Q<IntegerField>("txtAmount");
            EnumField = container.Q<EnumField>();

            Toggle.AddToClassList("Disable");
        }
    }
}
