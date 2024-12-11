using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System.Collections;
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
        public List<int> Ids;
    }

    struct Equipment
    {
        public Inventory inventory;
        public Dictionary<int, bool> equipment;
        public Dictionary<EquipmentType, GameObject> bodyParts;
    }
}
