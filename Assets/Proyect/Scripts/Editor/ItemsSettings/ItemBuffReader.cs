using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System.Collections.Generic;
using System.Linq;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class ItemBuffReader : BaseItemSetting
    {
        protected BuffsNamesDataArgs _changesBuffsIds;

        public BuffAdderUI BuffAdder { get; protected set; }

        public (InventoryItem, BuffsNamesDataArgs) GetBuffsIds(ElementType type)
        {
            return ((InventoryItem, BuffsNamesDataArgs))GetInfo(GetCreatedEnums(ElementType.Buff));
        }

        public CreatedBuffsDataArgs GetCreatedEnums(ElementType type)
        {
            var creations = new List<CreationData>();

            if (!SavingSystem.Data.creations.ContainsKey(type)) return new(null);

            foreach (var creation in SavingSystem.Data.creations[type])
            {
                creations.Add(creation.Value);
            }

            return new CreatedBuffsDataArgs(creations.ToArray());
        }

        protected void UpdateBuffs(in BuffData[] buffs, BuffsNamesDataArgs buffArgs)
        {
            if (buffArgs == null) return;

            List<(string id, BuffData? data)> buffsData = new();
            int consumableIdx = 0;

            foreach (var value in buffArgs.BuffsNames)
            {
                if (value == "")
                {
                    buffsData.Add((value, buffs[consumableIdx++]));
                    //continue;
                    //buffsData.Add((null, null));
                }
                else
                {
                    buffsData.Add((value, null));
                }
            }

            BuffAdder.UpdateData(buffsData);
        }

        protected bool CheckBuffChanges()
        {
            var namedBuffs = BuffAdder.GetBuffsData();

            if (namedBuffs.Count != _changesBuffsIds.BuffsNames.Count) return true;

            for (int i = 0, j = 0; i < namedBuffs.Count && j < _changesBuffsIds.BuffsNames.Count; i++)
            {
                if (namedBuffs[i].Name != _changesBuffsIds.BuffsNames[j])
                    return true;
            }

            return true;
        }

        protected List<BuffData> SelectCustomBuffs(List<NamedBuff> localBuffs)
        {
            List<BuffData> buffsData = new();

            foreach (NamedBuff curLocalBuff in localBuffs)
            {
                if (curLocalBuff.Name == "")
                    buffsData.Add(curLocalBuff.Data.Value);
            }

            return buffsData;
        }

        public (List<BuffData> buffsList, BuffsNamesDataArgs NamesList) GetBuffsInfo()
        {
            List<NamedBuff> curBuffs = BuffAdder.GetBuffsData();
            List<BuffData> buffs = SelectCustomBuffs(curBuffs);

            return ((buffs, new BuffsNamesDataArgs((from n in curBuffs select n.Name).ToList())));
        }
    }
}
