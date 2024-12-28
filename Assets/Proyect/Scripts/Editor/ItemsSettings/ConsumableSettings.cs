using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public class ConsumableSettings : BaseItemSetting
    {
        public FloatField ConsumptionTime { get; private set; }
        public FloatField AreaRadious { get; private set; }

        public override void Initialize(VisualElement container)
        {
            base.Initialize(container);

            ConsumptionTime = container.Q<FloatField>("ffConsumptionTime");
            AreaRadious = container.Q<FloatField>("ffAreaRadious");
        }
    }
}