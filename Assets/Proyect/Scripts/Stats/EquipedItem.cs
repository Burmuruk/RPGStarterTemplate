using Burmuruk.Tesis.Control;
using System;
using System.Collections.Generic;

namespace Burmuruk.Tesis.Stats
{
    public class EquipedItem : ISaveableItem
    {
        ItemType itemType;
        bool isEquipped;
        int count;
        List<Character> characters;
        int subType;

        public event Action<Character, EquipedItem> OnUnequiped;

        public ItemType ItemType { get => itemType; }
        public int Count { get => count; }
        public bool IsEquip { get => isEquipped; }
        public List<Character> Characters { get => characters; }

        public EquipedItem(ISaveableItem item, ItemType itemType)
        {
            this.itemType = itemType;
            subType = item.GetSubType();
            isEquipped = false;
            count = 0;
            characters = new();
            OnUnequiped = delegate { };
        }

        public EquipedItem(ItemType itemType, int subType)
        {
            this.itemType = itemType;
            this.subType = subType;
            isEquipped = false;
            count = 0;
            characters = new();
            OnUnequiped = delegate { };
        }

        public EquipedItem(ISaveableItem item, ItemType itemType, params Character[] characters) : this(item, itemType)
        {
            this.characters = new List<Character>(characters);
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
            this.characters.Add(character);

            if (!value)
            {
                OnUnequiped(character, this);
                characters.Remove(character);
            }
        }

        public void Unequip(Character character)
        {
            characters.Remove(character);

            if (characters.Count == 0)
                isEquipped = false;
        }

        public int GetSubType()
        {
            return subType;
        }

        public string GetName()
        {
            return "";
        }

        public string GetDescription()
        {
            return "";
        }
    }
}
