using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Editor
{
    public class CharacterTag : ScriptableObject
    {
        public List<CharacterProfile> characters = new();
        public Dictionary<ElementType, List<string>> elements = new()
        {
            { ElementType.Component, new() 
                { 
                    "Movement",
                    "Health",
                    "Fighter",
                    "Inventory",
                    "Equipment",
                } 
            },
        };
        public string[] ItemTypes = new[]
        {
            "Saveable",
            "Hability",
            "Equipable",
            "Consumable",
        };
        public List<string> bodyParts = new()
        {
            "Body",
            "Head",
            "RightHand",
            "Chest",
            "Legs",
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
    }

}