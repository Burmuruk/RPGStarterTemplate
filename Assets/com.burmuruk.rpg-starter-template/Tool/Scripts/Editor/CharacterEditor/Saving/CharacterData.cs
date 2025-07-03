using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Editor
{
    public struct CharacterData
    {
        public Type className;
        public string characterName;
        public Color color;
        public CharacterType characterType;
        public string enemyTag;
        public bool shouldSave;
        public Dictionary<ComponentType, object> components;
        public CharacterProgress progress;
        public BasicStats basicStats;
    }

    public struct CharacterComponent
    {
        public object data;
        public bool shouldSave;
    }

    public struct Inventory
    {
        public bool addInventory;
        public Dictionary<string, int> items;
    }

    public struct Equipment
    {
        public GameObject model;
        public List<(Transform transform, EquipmentType type)> spawnPoints;
        public Inventory inventory;
        public Dictionary<string, EquipData> equipment;

        public Equipment(Inventory inventory)
        {
            this.inventory = inventory;
            equipment = new();
            model = null;
            spawnPoints = null;
        }
    }

    public struct EquipData
    {
        public ElementType type;
        public EquipmentType place;
        public bool equipped;
    }

    [Serializable]
    public struct Health
    {
        public int HP;
        public int MaxHP;
    }
}

//using Burmuruk.Tesis.Inventory;
//using Burmuruk.Tesis.Stats;
//using System;
//using System.Collections.Generic;
//using UnityEngine;

//namespace Burmuruk.Tesis.Editor
//{
//    [Serializable]
//    public struct CharacterData
//    {
//        public Type className;
//        public string characterName;
//        public Color color;
//        public CharacterType characterType;
//        public string enemyTag;
//        public bool shouldSave;
//        public List<Health> health;
//        public List<Equipment> equipment;
//        public List<Inventory> inventory;
//        public CharacterProgress progress;
//        public BasicStats basicStats;
//    }

//    [Serializable]
//    public class CharacterComponent
//    {
//        public ComponentType type;

//    }

//    [Serializable]
//    public struct Inventory
//    {
//        public List<ItemData> items;
//    }

//    [Serializable]
//    public struct ItemData
//    {
//        public string name;
//        public int amount;
//    }

//    [Serializable]
//    public struct Equipment
//    {
//        public GameObject model;
//        public List<SpawnPointData> spawnPoints;
//        public Inventory inventory;
//        public Dictionary<string, EquipData> equipment;

//        public Equipment(Inventory inventory)
//        {
//            this.inventory = inventory;
//            equipment = new();
//            model = null;
//            spawnPoints = null;
//        }
//    }

//    [Serializable]
//    public struct SpawnPointData
//    {
//        public Transform transform;
//        public EquipmentType type;
//        public SpawnPointData(Transform transform, EquipmentType type)
//        {
//            this.transform = transform;
//            this.type = type;
//        }
//    }

//    [Serializable]
//    public struct EquipData
//    {
//        public ElementType type;
//        public EquipmentType place;
//        public bool equipped;
//    }

//    [Serializable]
//    public struct Health
//    {
//        public int HP;
//        public int MaxHP;
//    }
//}

