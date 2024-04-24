using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Burmuruk.Tesis.Stats.Inventary;

namespace Burmuruk.Tesis.Stats
{
    public interface IInventary
    {
        public Weapon EquipedWeapon { get; }
        public bool Add(ItemType type, ISaveableItem item);
        public void Add(ItemType itemType, int idx);
        public bool Remove(ItemType type, int idx);
        public List<ISaveableItem> GetOwnedList(ItemType type);
        public ISaveableItem GetOwnedItem(ItemType type, int idx);
    }
}
