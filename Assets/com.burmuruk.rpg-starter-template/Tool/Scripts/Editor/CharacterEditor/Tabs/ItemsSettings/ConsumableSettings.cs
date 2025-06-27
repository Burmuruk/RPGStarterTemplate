using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System.Collections.Generic;
using UnityEngine.UIElements;
using static Burmuruk.Tesis.Editor.Utilities.UtilitiesUI;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class ConsumableSettings : ItemBuffReader
    {
        public FloatField ConsumptionTime { get; private set; }
        public FloatField AreaRadious { get; private set; }

        public override void Initialize(VisualElement container, CreationsBaseInfo name)
        {
            base.Initialize(container, name);

            BuffAdder = new BuffAdderUI(container);
            ConsumptionTime = container.Q<FloatField>("ffConsumptionTime");
            AreaRadious = container.Q<FloatField>("ffAreaRadious");
        }

        public override void UpdateInfo(InventoryItem data, ItemDataArgs args, ItemType type = ItemType.Consumable)
        {
            _changes = new ConsumableItem();
            base.UpdateInfo(data, args, type);
            var consumable = data as ConsumableItem;
            var buffArgs = args as BuffsNamesDataArgs;

            if (consumable == null) return;

            ConsumptionTime.value = consumable.ConsumptionTime;
            AreaRadious.value = consumable.AreaRadious;
            (_changes as ConsumableItem).UpdateInfo(consumable.Buffs, ConsumptionTime.value, AreaRadious.value);

            UpdateBuffs(consumable.Buffs, buffArgs);
        }

        public override (InventoryItem item, ItemDataArgs args) GetInfo(ItemDataArgs args)
        {
            ConsumableItem newItem = new();
            newItem.Copy(base.GetInfo(null).item);

            (var buffs, var buffsNames) = GetBuffsInfo();

            newItem.UpdateInfo(
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

            _changes = null;
        }

        public override void Remove_Changes()
        {
            base.Remove_Changes();
            BuffAdder.Remove_Changes();
        }

        #region Saving
        public override ModificationTypes Check_Changes()
        {
            try
            {
                if (_changes == null) return CurModificationType = ModificationTypes.Add;

                CurModificationType = ModificationTypes.None;
                var lastData = _changes as ConsumableItem;
                base.Check_Changes();

                if (BuffAdder.Check_Changes() != ModificationTypes.None)
                {
                    CurModificationType = ModificationTypes.EditData;
                }

                if (ConsumptionTime.value != lastData.ConsumptionTime)
                {
                    Highlight(ConsumptionTime, true, BorderColour.HighlightBorder);

                    CurModificationType = ModificationTypes.EditData;
                }

                if (AreaRadious.value != lastData.AreaRadious)
                {
                    Highlight(AreaRadious, true, BorderColour.HighlightBorder);

                    CurModificationType = ModificationTypes.EditData;
                }

                return CurModificationType;
            }
            catch (InvalidDataExeption e)
            {
                throw e;
            }
        }

        public override void Load_Changes()
        {
            base.Load_Changes();

            var changes = _changes as ConsumableItem;

            BuffAdder.Load_Changes();
            ConsumptionTime.value = changes.ConsumptionTime;
            AreaRadious.value = changes.AreaRadious;
        }

        public override bool Save()
        {
            if (!VerifyData())
            {
                Utilities.UtilitiesUI.Notify("Invalid Data", BorderColour.Error);
                return false;
            }

            try
            {
                if (_creationsState == CreationsState.Editing && Check_Changes() == ModificationTypes.None)
                {
                    Notify("No changes were found", BorderColour.HighlightBorder);
                    return false;
                }
                else
                    CurModificationType = ModificationTypes.Add;

                DisableNotification();
                var data = GetBuffsIds();
                var creationData = new CreationData(TxtName.text.Trim(), data);

                return SavingSystem.SaveCreation(ElementType.Consumable, in _id, in creationData, CurModificationType);
            }
            catch (InvalidDataExeption e)
            {
                throw e;
            }
        }

        public override CreationData Load(ElementType type, string id)
        {
            CreationData? result = SavingSystem.Load(type, id);

            if (!result.HasValue) return default;

            _id = id;
            (var item, var args) = ((InventoryItem, BuffsNamesDataArgs))result.Value.data;
            Set_CreationState(CreationsState.Editing);
            UpdateInfo(item, args);

            return result.Value;
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