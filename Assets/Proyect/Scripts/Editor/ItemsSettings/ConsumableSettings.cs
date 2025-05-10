using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class ConsumableSettings : ItemBuffReader
    {
        ConsumableItem _changes;

        public FloatField ConsumptionTime { get; private set; }
        public FloatField AreaRadious { get; private set; }

        public override void Initialize(VisualElement container, NameSettings name)
        {
            base.Initialize(container, name);

            BuffAdder = new BuffAdderUI(container);
            ConsumptionTime = container.Q<FloatField>("ffConsumptionTime");
            AreaRadious = container.Q<FloatField>("ffAreaRadious");
            _changes = new ConsumableItem();
        }

        public override void UpdateInfo(InventoryItem data, ItemDataArgs args)
        {
            base.UpdateInfo(data, args);
            var consumable = data as ConsumableItem;
            var buffArgs = args as BuffsNamesDataArgs;

            if (consumable == null) return;

            ConsumptionTime.value = consumable.ConsumptionTime;
            AreaRadious.value = consumable.AreaRadious;

            UpdateBuffs(consumable.Buffs, buffArgs);
        }

        public override (InventoryItem item, ItemDataArgs args) GetInfo(ItemDataArgs args)
        {
            ConsumableItem newItem = new();
            newItem.Copy(base.GetInfo(null).item);

            (var buffs, var buffsNames) = GetBuffsInfo();

            newItem.Populate(
                buffs.ToArray(),
                ConsumptionTime.value,
                AreaRadious.value
                );

            return (newItem, buffsNames);
        }

        public override void Clear()
        {
            base.Clear();
            BuffAdder.Clear();
            ConsumptionTime.value = 0;
            AreaRadious.value = 0;
        }

        #region Saving
        public override ModificationType Check_Changes()
        {
            //throw new InvalidNameExeption();

            if ((_nameControl.Check_Changes() | ModificationType.None) != 0)
            {
                CurModificationType = ModificationType.Rename;
            }

            if ((BuffAdder.Check_Changes() | ModificationType.None) != 0)
            {
                CurModificationType = ModificationType.EditData;
            }

            if (ConsumptionTime.value != _changes.ConsumptionTime)
            {
                Highlight(ConsumptionTime, true, BorderColour.HighlightBorder);

                CurModificationType = ModificationType.EditData;
            }
            if (AreaRadious.value != _changes.AreaRadious)
            {
                Highlight(AreaRadious, true, BorderColour.HighlightBorder);

                CurModificationType = ModificationType.EditData;
            }

            return CurModificationType;
        }

        public override string Save()
        {
            if (Check_Changes()) return null;

            var data = GetBuffsIds(ElementType.Buff);
            CreationData creationData = new CreationData(TxtName.text, data);

            return SavingSystem.SaveCreation(ElementType.Buff, in _id, in creationData);
        }

        public override CreationData Load(ElementType type, string id)
        {
            return SavingSystem.Load(type, id).Value;
        }
        #endregion
    }

    public record BuffsNamesDataArgs : ItemDataArgs
    {
        public List<string> BuffsNames { get; private set; }

        public BuffsNamesDataArgs(List<string> buffs)
        {
            BuffsNames = buffs;
        }
    }

    public record BuffsIdsDataArgs : ItemDataArgs
    {
        public List<(string name, string id)> BuffsNames { get; private set; }

        public BuffsIdsDataArgs(List<(string, string)> buffs)
        {
            BuffsNames = buffs;
        }
    }

    public record CreatedBuffsDataArgs : ItemDataArgs
    {
        public CreationData[] Buffs { get; private set; }

        public CreatedBuffsDataArgs(CreationData[] buffs)
        {
            Buffs = buffs;
        }
    }

    public struct NamedBuff
    {
        public string Name { get; set; }
        public BuffData? Data { get; private set; }

        public NamedBuff(string name, BuffData? data)
        {
            Name = name;
            Data = data;
        }
    }
}