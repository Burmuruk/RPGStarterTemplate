using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Editor
{
    public class CharacterTag : ScriptableObject
    {
        public List<CharacterProfile> characters = new();
        public Dictionary<ElementType, Dictionary<string, object>> creations = new();
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
        public Dictionary<ElementType, List<string>> elements = new()
        {
            {
                ElementType.Character, new()
                {
                    "Mage",
                    "Tank",
                    "Healer",
                    "Midget",
                    "Assasin",
                }
            },
            {
                ElementType.Item, new()
                {
                    "Potion",
                    "Cake",
                }
            },
            {
                ElementType.Weapon, new()
                {
                    "Hammer",
                    "Sword",
                    "GreatSword",
                    "Dagger",
                }
            },
            //{
            //    ElementType.State, new()
            //    {
            //        "Madness",
            //        "Sleep",
            //    }
            //},
        };

        [Serializable]
        public struct CharacterProfile
        {
            public string name;
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