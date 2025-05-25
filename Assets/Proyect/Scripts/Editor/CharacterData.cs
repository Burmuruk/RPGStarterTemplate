using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Editor
{
    public struct CharacterData
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

    public struct Inventory
    {
        public Dictionary<string, int> items;
    }

    public struct Equipment
    {
        public Inventory inventory;
        public Dictionary<string, EquipData> equipment;

        public Equipment(Inventory inventory)
        {
            this.inventory = inventory;
            equipment = new();
        }
    }

    public struct EquipData
    {
        public ElementType type;
        public EquipmentType place;
        public bool equipped;
    }
}
