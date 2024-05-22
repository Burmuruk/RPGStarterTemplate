using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Burmuruk.Tesis.Stats.Inventory;

namespace Burmuruk.Tesis.Stats
{
    public interface IInventory
    {
        public Weapon EquipedWeapon { get; }
        public bool Add(ItemType type, ISaveableItem item);
        public bool Remove(ItemType type, int idx);
        public List<ISaveableItem> GetOwnedList(ItemType type);
        public List<ISaveableItem> GetList(ItemType type);
        public ISaveableItem GetOwnedItem(ItemType type, int idx);
        public ISaveableItem GetItem(ItemType type, int idx);
        public int GetItemCount(ItemType type, int subType);
        public int GetItemMaxCount(ItemType type, int subType);
    }
}
