using Burmuruk.Tesis.Control;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    public class InventaryEquipDecorator : MonoBehaviour, IInventary
    {
        Inventary inventary;
        PlayerCustomizationManager customazationManager;

        (ItemType itemType, EquipedItem item) alarmedRemovedItem = default;
        (ItemType itemType, Character player, EquipedItem item) alarmedEquipItem = default;

        public event Action OnTryDeleteEquiped;
        public event Action OnTryAlreadyEquiped;

        public Weapon EquipedWeapon { get => inventary.EquipedWeapon; }

        private void Start()
        {
            customazationManager = GetComponent<PlayerCustomizationManager>();
        }

        public void SetInventary(Inventary inventary) => this.inventary = inventary;

        public void Equip(Character player, ItemType itemType, int type)
        {
            var item = inventary.GetOwnedItem(itemType, type);

            if (item == null) return;

            var equiped = (EquipedItem)item;
            if (equiped.Characters.Contains(player)) return;

            alarmedEquipItem = (itemType, player, (EquipedItem)item);

            if (equiped.IsEquip && !CheckHaveMoreItems(itemType, type, equiped))
            {
                OnTryAlreadyEquiped?.Invoke();
                return;
            }

            EquipDirec();

            bool CheckHaveMoreItems(ItemType itemType, int type, EquipedItem equiped)
            {
                return inventary.GetItemCount(itemType, type) > equiped.Characters.Count;
            }
        }

        public void EquipDirec()
        {
            var (itemType, player, equipedItem) = alarmedEquipItem;
            equipedItem.Equip(player);
            
            var prefab = inventary.GetItem(itemType, equipedItem.GetSubType());

            if (prefab is IEquipable equipable && equipable != null)
            {
                customazationManager.EquipModification(player, equipable);
            }

            alarmedEquipItem = default; 
        }

        public void Unequip(Character player, EquipedItem item)
        {
            if (!item.Characters.Contains(player)) return;

            item.Unequip(player);
            //item.OnUnequiped -= Unequip;
            var equipedItem = inventary.GetItem(item.Type, item.GetSubType()) as IEquipable;

            customazationManager.UnequipModification(player, equipedItem);
        }

        public bool Add(ItemType type, ISaveableItem item)
        {
            var equipedItem = new EquipedItem(item, type);

            return inventary.Add(type, equipedItem);
        }

        public void Add(ItemType itemType, int idx)
        {
            inventary.Add(itemType, new EquipedItem(itemType, idx));
        }

        public bool Remove(ItemType type, int idx)
        {
            var item = inventary.GetOwnedItem(type, idx);

            if (item != null) return false;

            alarmedRemovedItem = (type, (EquipedItem)item);

            if (((EquipedItem)item).IsEquip)
            {
                OnTryDeleteEquiped?.Invoke();
                return false;
            }

            RemoveDirect();

            return true;
        }

        public void RemoveDirect()
        {
            if (alarmedRemovedItem.item == null) return;

            inventary.Remove(alarmedRemovedItem.itemType, alarmedRemovedItem.item.GetSubType());

            alarmedRemovedItem = default;
        }

        public ISaveableItem GetOwnedItem(ItemType type, int idx)
        {
            return inventary.GetOwnedItem(type, idx);
        }

        public List<ISaveableItem> GetOwnedList(ItemType type)
        {
            return inventary.GetOwnedList(type);
        }

        public List<ISaveableItem> GetEquipedItems(ItemType itemType, Character character)
        {
            var items = inventary.GetOwnedList(itemType);

            List<ISaveableItem> equipedItems = new(); 

            foreach (var item in items)
            {
                var equiped = item as EquipedItem;
                if (equiped.IsEquip && equiped.Characters.Contains(character))
                {
                    equipedItems.Add(equiped);
                }
            }

            return equipedItems;
        }

        public List<ISaveableItem> GetList(ItemType type)
        {
            return inventary.GetList(type);
        }

        public ISaveableItem GetItem(ItemType type, int idx)
        {
            return inventary.GetItem(type, idx);
        }

        public bool Use(ItemType type, int subType)
        {
            

            return true;
        }

        public int GetItemCount(ItemType type, int subType)
        {
            return inventary.GetItemCount(type, subType);
        }

        public int GetItemMaxCount(ItemType type, int subType)
        {
            return inventary.GetItemMaxCount(type, subType);
        }
    }
}
