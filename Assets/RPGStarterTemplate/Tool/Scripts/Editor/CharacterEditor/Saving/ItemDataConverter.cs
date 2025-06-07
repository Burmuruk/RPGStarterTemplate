using Burmuruk.Tesis.Editor.Controls;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System.Collections.Generic;

namespace Burmuruk.Tesis.Editor
{
    public class ItemDataConverter
    {
        public static InventoryItem GetItem(ElementType type, string id)
        {
            var creation = SavingSystem.Data.creations[type][id];

            switch (type)
            {
                case ElementType.Item:
                case ElementType.Armour:
                    var (item, _) = ((InventoryItem, ItemDataArgs))creation.data;
                    return item;

                case ElementType.Weapon:
                case ElementType.Consumable:
                    var (buffUser, cArgs) = ((InventoryItem, BuffsNamesDataArgs))creation.data;
                    Update_BuffsInfo(buffUser as IBuffUser, cArgs);

                    return buffUser;

                default:
                    return null;
            }
        }

        private static void Update_BuffsInfo(IBuffUser buffUser, BuffsNamesDataArgs args)
        {
            List<BuffData> newBuffs = new();
            int idx = 0;

            foreach (var name in args.BuffsNames)
            {
                if (name == "")
                {
                    BuffData newBuff = buffUser.Buffs[idx];
                    newBuff.name = "Custom";
                    newBuffs.Add(newBuff);
                    ++idx;
                }
                else
                {
                    newBuffs.Add((BuffData)SavingSystem.Data.creations[ElementType.Buff][name].data);
                }
            }

            buffUser.UpdateBuffData(newBuffs.ToArray());
        }
    }
}
