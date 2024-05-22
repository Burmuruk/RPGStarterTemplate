using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Burmuruk.Tesis.Stats
{
    public class PlayerInventary
    {
        public Inventory MainInventary { get; set; }

        public bool Add(ItemType type, ISaveableItem item)
        {
            return MainInventary.Add(type, item);
        }

        public bool Remove(ItemType type, int idx)
        {
            return MainInventary.Remove(type, idx);
        }

        public ISaveableItem GetOwnedItem(ItemType type, int idx)
        {
            throw new NotImplementedException();
        }
    }
}
