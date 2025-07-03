using Burmuruk.Tesis.Editor.Controls;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Editor
{
    public class CreationDatabase : ScriptableObject
    {
        public List<CharacterProfile> characters = new();
        public Dictionary<ElementType, Dictionary<string, CreationData>> creations = new();
        public Dictionary<ElementType, Dictionary<string, CreationData>> changes = new();
        public ElementType mainElementChange = ElementType.None;
        public List<ElementType> defaultElements = new()
        {
            ElementType.Character,
            ElementType.Item,
            ElementType.Consumable,
            ElementType.Weapon,
            ElementType.Armour,
            //ElementType.State,
            ElementType.Buff,
        };
        string _creationPath;
        [SerializeField]
        private List<ElementEntry> serializedCreations = new();

        public string CreationPath
        {
            get => _creationPath;
            set
            {
                _creationPath = value;
            }
        }

        [Serializable]
        public struct CharacterProfile
        {
            public string name;
        }

        public bool TryGetCreation(string name, ElementType type, out string id)
        {
            id = null;
            if (!creations.ContainsKey(type)) return false;

            foreach (var creation in creations[type])
            {
                if (creation.Value.Name == name)
                {
                    id = creation.Key;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetCreation(string id, out CreationData data, out ElementType type)
        {
            data = default;
            type = default;

            foreach (var key in creations.Keys)
            {
                foreach (var creation in creations[key])
                {
                    if (creation.Key == id)
                    {
                        data = creation.Value;
                        type = key;
                        return true;
                    }
                }
            }

            return true;
        }

        public void SyncFromSerialized()
        {
            creations.Clear();
            //serializedCreations.Clear();
            foreach (var entry in serializedCreations)
            {
                var innerDict = new Dictionary<string, CreationData>();
                foreach (var item in entry.items)
                {
                    innerDict[item.name] = item.data;
                }
                creations[entry.type] = innerDict;
            }
        }

        public void SyncToSerialized()
        {
            serializedCreations.Clear();
            foreach (var kvp in creations)
            {
                var entry = new ElementEntry
                {
                    type = kvp.Key,
                    items = new List<NamedData>()
                };

                foreach (var inner in kvp.Value)
                {
                    entry.items.Add(new NamedData { name = inner.Key, data = inner.Value });
                }

                serializedCreations.Add(entry);
            }
        }

    }

    [Serializable]
    public class ElementEntry
    {
        public ElementType type;
        public List<NamedData> items = new();
    }

    [Serializable]
    public class NamedData
    {
        public string name;

        [SerializeReference]
        public CreationData data;
    }





    [Serializable]
    public class CreationData
    {
        public string Name;

        public CreationData(string name)
        {
            Name = name;
        }
    }

    [Serializable]
    public class BuffCreationData : CreationData
    {
        public Stats.BuffData Data;

        public BuffCreationData(string name, Stats.BuffData data) : base(name)
        {
            Data = data;
        }
    }

    [Serializable]
    public class ItemCreationData : CreationData
    {
        public Tesis.Inventory.InventoryItem Data;

        public ItemCreationData(string name, Tesis.Inventory.InventoryItem data) : base(name)
        {
            Data = data;
        }
    }














    [Serializable]
    public class BuffUserCreationData : CreationData
    {
        public Tesis.Inventory.InventoryItem Data;
        public BuffsNamesDataArgs Names;

        public BuffUserCreationData(string name, Tesis.Inventory.InventoryItem item, BuffsNamesDataArgs args) : base(name)
        {
            Data = item;
            Names = args;
        }
    }

    [Serializable]
    public class CharacterCreationData : CreationData
    {
        public CharacterData Data;

        public CharacterCreationData(string name, CharacterData data) : base(name)
        {
            Data = data;
        }
    }

    public enum ElementType
    {
        None,
        Component,
        Item,
        Character,
        Buff,
        //Mod,
        //State,
        Ability,
        Creation,
        Weapon,
        Armour,
        Consumable,
    }

    public enum ComponentType
    {
        None,
        Health,
        Fighter,
        Mover,
        Inventory,
        Equipment,
        //Flying,
        //Dialogue,
        //Patrolling,
    }
}