using Burmuruk.Tesis.Control;
using Burmuruk.Tesis.Inventory;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Burmuruk.Tesis.Inventory
{
    public abstract class EquipeableItem : InventoryItem
    {
        int maxCount;
        List<Character> characters;

        public event Action<Character, EquipeableItem> OnUnequiped;

        public int MaxCount { get => maxCount; }
        public bool IsEquip { get => characters.Count > 0; }
        public List<Character> Characters { get => characters; }

        public EquipeableItem(params Character[] characters)
        {
            this.characters = new List<Character>(characters);
        }

        public EquipeableItem(int count, params Character[] characters) : this (characters)
        {
            this.maxCount = count;
        }

        public abstract object GetEquipLocation();

        public void Equip(Character character)
        {
            characters.Add(character);
        }

        public void Unequip(Character character)
        {
            OnUnequiped(character, this);
            characters.Remove(character);
        }
    }
}
