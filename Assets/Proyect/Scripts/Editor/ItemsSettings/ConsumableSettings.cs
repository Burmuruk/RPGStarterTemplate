using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public class ConsumableSettings : BaseItemSetting
    {
        public FloatField ConsumptionTime { get; private set; }
        public FloatField AreaRadious { get; private set; }


        public override void Initialize(VisualElement container, TextField name)
        {
            base.Initialize(container, name);

            ConsumptionTime = container.Q<FloatField>("ffConsumptionTime");
            AreaRadious = container.Q<FloatField>("ffAreaRadious");

            BuffAdderUI buffAdder = new BuffAdderUI(container);
        }
    }
}