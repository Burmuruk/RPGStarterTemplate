using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Editor
{
    struct CharacterData
    {
        public string characterName;
        public Color color;
        public BasicStats stats;
        public CharacterType characterType;
        public Dictionary<ComponentType, object> components;
    }

    struct Health
    {
        public int HP;
    }

    struct Inventory
    {
        public Dictionary<ElementType, int> items;
    }

    struct Equipment
    {
        public Inventory inventory;
        public Dictionary<ElementType, EquipData> equipment;

        public Equipment(Inventory inventory)
        {
            this.inventory = inventory;
            equipment = new();
        }
    }

    struct EquipData
    {
        public ElementType type;
        public EquipmentType place;
        public bool equipped;
    }
}
