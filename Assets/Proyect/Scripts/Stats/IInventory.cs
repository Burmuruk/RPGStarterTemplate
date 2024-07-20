using System.Collections.Generic;

namespace Burmuruk.Tesis.Inventory
{
    public interface IInventory
    {
        public bool Add(int id);
        public bool Remove(int id);
        public List<InventoryItem> GetOwnedList(ItemType type);
        public InventoryItem GetOwnedItem(int id);
        public int GetItemCount(int id);
    }
}
