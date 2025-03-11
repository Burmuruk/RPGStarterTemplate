using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public class ConsumableSettings : BaseItemSetting
    {
        public BuffAdderUI BuffAdder { get; private set; }
        public FloatField ConsumptionTime { get; private set; }
        public FloatField AreaRadious { get; private set; }


        public override void Initialize(VisualElement container, TextField name)
        {
            base.Initialize(container, name);

            BuffAdder = new BuffAdderUI(container);
            ConsumptionTime = container.Q<FloatField>("ffConsumptionTime");
            AreaRadious = container.Q<FloatField>("ffAreaRadious");
        }

        public override void UpdateInfo(InventoryItem data, ItemDataArgs args)
        {
            base.UpdateInfo(data, args);
            var consumable = data as ConsumableItem;
            var buffArgs = args as BuffsNamesDataArgs;

            if (consumable == null) return;

            ConsumptionTime.value = consumable.ConsumptionTime;
            AreaRadious.value = consumable.AreaRadious;

            UpdateBuffs(consumable, buffArgs);
        }

        private void UpdateBuffs(ConsumableItem consumable, BuffsNamesDataArgs buffArgs)
        {
            if (buffArgs == null) return;

            List<(string, BuffData?)> buffsData = new();
            int i = 0;
            int consumableIdx = 0;

            foreach (var name in buffArgs.BuffsNames)
            {
                if (name is null || name == BuffAdderUI.INVALIDNAME)
                {
                    buffsData.Add((BuffAdderUI.INVALIDNAME, consumable.Buffs[consumableIdx++]));
                    //continue;
                    //buffsData.Add((null, null));
                }
                else
                {
                    buffsData.Add((name, null));
                }

                ++i;
            }

            BuffAdder.UpdateData(buffsData);
        }

        public override (InventoryItem item, ItemDataArgs args) GetInfo(ItemDataArgs args)
        {
            var buffArgs = args as CreatedBuffsDataArgs;

            if (buffArgs == null) return default;

            ConsumableItem newItem = new();
            newItem.Copy(base.GetInfo(null).Item1);

            List<NamedBuff> curBuffs = (from buff in BuffAdder.GetBuffsData()
                                        where !string.IsNullOrEmpty(buff.Name)
                                        select buff).ToList();
            var buffs = SelectCustomBuffs(curBuffs);

            newItem.Populate(
                buffs.ToArray(),
                ConsumptionTime.value,
                AreaRadious.value
                );

            return (newItem, new BuffsNamesDataArgs((from n in curBuffs select n.Name).ToList()));
        }

        public override void Clear()
        {
            base.Clear();
            BuffAdder.Clear();
            ConsumptionTime.value = 0;
            AreaRadious.value = 0;
        }

        private List<BuffData> SelectCustomBuffs(List<NamedBuff> localBuffs)
        {
            List<BuffData> buffsData = new();

            foreach (NamedBuff curLocalBuff in localBuffs)
            {
                //if (curLocalBuff.Name != BuffAdderUI.INVALIDNAME)
                //{
                //    if (registeredBuffs != null)
                //    {
                //        foreach (var registeredBuff in registeredBuffs)
                //        {
                //            if (registeredBuff.Name == curLocalBuff.Name)
                //            {
                //                buffsData.Add(default);
                //                break;
                //            }
                //        }
                //    }

                //    continue;
                //}

                if (curLocalBuff.Name == BuffAdderUI.INVALIDNAME)
                    buffsData.Add(curLocalBuff.Data.Value);
            }

            return buffsData;
        }
    }

    public record BuffsNamesDataArgs : ItemDataArgs
    {
        public List<string> BuffsNames { get; private set; }

        public BuffsNamesDataArgs(List<string> buffs)
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
        public string Name { get; private set; }
        public BuffData? Data { get; private set; }

        public NamedBuff (string name, BuffData? data)
        {
            Name = name;
            Data = data;
        }
    }
}