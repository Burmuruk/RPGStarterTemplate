using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Editor
{
    public class CharacterTag : ScriptableObject
    {
        public List<CharacterProfile> characters = new();
        public List<ElementType> defaultElements = new()
        {
            ElementType.Character,
            ElementType.Item,
            ElementType.Consumable,
            ElementType.Weapon,
            ElementType.Armor,
            ElementType.State,
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
            {
                ElementType.State, new()
                {
                    "Madness",
                    "Sleep",
                }
            },
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
        State,
        Hability,
        Creation,
        Weapon,
        Armor,
        Consumable,
    }
}