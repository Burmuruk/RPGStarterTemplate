using Burmuruk.Tesis.Control;
using System;

namespace Burmuruk.Tesis.Stats
{
    public class EquipedItem : ISaveableItem
    {
        ItemType itemType;
        bool isEquipped;
        int count;
        Character character;
        int subType;

        public event Action<Character, EquipedItem> OnUnequiped;

        public ItemType ItemType { get => itemType; }
        public int Count { get => count; }
        public bool IsEquip { get => isEquipped; }

        public EquipedItem(ISaveableItem item, ItemType itemType)
        {
            this.itemType = itemType;
            subType = item.GetSubType();
            isEquipped = false;
            count = 0;
            character = null;
            OnUnequiped = delegate { };
        }

        public EquipedItem(ItemType itemType, int subType)
        {
            this.itemType = itemType;
            this.subType = subType;
            isEquipped = false;
            count = 0;
            character = null;
            OnUnequiped = delegate { };
        }

        public EquipedItem(ISaveableItem item, ItemType itemType, Character character) : this(item, itemType)
        {
            this.character = character;
        }

        public EquipedItem(ISaveableItem item, ItemType itemType, int count) : this(item, itemType)
        {
            this.count = count;
        }

        public void Add(int amount = 1) => count += amount;
        public void Remove(int amount = 1) => count -= amount;

        public void Equip(bool value, Character character)
        {
            isEquipped = value;
            this.character = character;

            if (!value)
                OnUnequiped(character, this);
        }

        public int GetSubType()
        {
            return subType;
        }
    }
}
