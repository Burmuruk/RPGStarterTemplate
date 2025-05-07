using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Editor
{
    public class CharacterTag : ScriptableObject
    {
        public List<CharacterProfile> characters = new();
        public Dictionary<ElementType, Dictionary<string, CreationData>> creations = new();
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

        [Serializable]
        public struct CharacterProfile
        {
            public string name;
        }

        public bool GetCreation(string name, ElementType type, out string id)
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
    }

    public struct CreationData
    {
        public string Name { get; private set; }
        public object data;

        public CreationData(string name, object data)
        {
            Name = name;
            this.data = data;
        }
    }

    public enum ElementType
    {
        None,
        Component,
        Item,
        Character,
        Buff,
        Mod,
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
        Dialogue,
        Patrolling,
    }
}