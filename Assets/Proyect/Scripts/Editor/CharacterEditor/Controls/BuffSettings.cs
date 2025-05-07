using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class BuffSettings : BaseItemSetting
    {
        public BuffData Buff { get; set; }
        public FloatField Value { get; private set; }
        public FloatField Duration { get; private set; }
        public FloatField Rate { get; private set; }
        public Toggle Percentage { get; private set; }
        public FloatField Probability { get; private set; }
        public EnumField Stat { get; private set; }
        private string buffName;

        public override void Initialize(VisualElement container, NameSettings name)
        {
            base.Initialize(container, name);

            Value = container.Q<FloatField>("ffConsumptionTime");
            Duration = container.Q<FloatField>("ffConsumptionTime");
            Rate = container.Q<FloatField>("ffConsumptionTime");
            Percentage = container.Q<Toggle>("ffConsumptionTime");
            Probability = container.Q<FloatField>("ffConsumptionTime");
            Stat = container.Q<EnumField>("ffConsumptionTime");

            Stat.Init(ModifiableStat.None);
        }

        public override (InventoryItem item, ItemDataArgs args) GetInfo(ItemDataArgs args)
        {
            return base.GetInfo(args);
        }

        public override void UpdateInfo(InventoryItem data, ItemDataArgs args)
        {
            base.UpdateInfo(data, args);
        }

        //public override bool CheckChanges(InventoryItem targetData, ItemDataArgs args, out List<VisualElement> controls, out ModificationType modType)
        //{
        //    controls = new List<VisualElement>();
        //    modType = ModificationType.None;

        //    if (Buff.stat == (ModifiableStat)Stat.value)
        //    {
        //        modType = ModificationType.EditData;
        //        controls.Add(Stat);
        //    }
        //    if (Buff.value == Value.value)
        //    {
        //        modType = ModificationType.EditData;
        //        controls.Add(Value);
        //    }
        //    if (Buff.duration == Duration.value)
        //    {
        //        modType = ModificationType.EditData;
        //        controls.Add(Duration);
        //    }
        //    if (Buff.rate == Rate.value)
        //    {
        //        modType = ModificationType.EditData;
        //        controls.Add(Rate);
        //    }
        //    if (Buff.percentage == Percentage.value)
        //    {
        //        modType = ModificationType.EditData;
        //        controls.Add(Percentage);
        //    }
        //    if (Buff.probability == Probability.value)
        //    {
        //        modType = ModificationType.EditData;
        //        controls.Add(Probability);
        //    }

        //    if (buffName == TxtName.value)
        //    {
        //        if (controls.Count == 0)
        //        {
        //            modType = ModificationType.Rename;
        //        }
        //        else
        //        {
        //            modType = ModificationType.EditData | ModificationType.Rename;
        //        }

        //        controls.Add(TxtName);
        //    }

        //    return controls.Count > 0;
        //}
    }

    public record BuffDataArgs : ItemDataArgs
    {
        public readonly BuffData Buff;

        public BuffDataArgs(BuffData buff)
        {
            Buff = buff;
        }
    }
}
