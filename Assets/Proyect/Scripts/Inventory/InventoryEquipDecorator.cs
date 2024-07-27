using Burmuruk.Tesis.Control;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Inventory
{
    public class InventoryEquipDecorator : MonoBehaviour, IInventory
    {
        [SerializeField] List<InitalEquipedItemData> initialItems;
        [SerializeField] Inventory inventory;
        Equipment equipment;

        [Serializable]
        struct InitalEquipedItemData
        {
            [SerializeField] InventoryItem item;
            [SerializeField] bool isEquip;
            [SerializeField] Character character;

            public readonly InventoryItem Item { get =>  item; }
            public readonly bool IsEquiped {  get => isEquip; }
            public readonly Character Character { get => character; }
        }

        EquipeableItem alarmedRemovedItem = default;
        (Character player, EquipeableItem item) alarmedEquipItem = default;
        public ref Equipment Equipped { get => ref equipment; }

        public event Action OnTryDeleteEquiped;
        public event Action OnTryAlreadyEquiped;

        private void Start()
        {
            InitInventory();
        }

        public void SetInventory(Inventory inventory) => this.inventory = inventory;

        public bool TryEquip(Character player, InventoryItem item, out List<EquipeableItem> unequippedItems)
        {
            unequippedItems = null;
            if (item == null) return false;

            var equiped = (EquipeableItem)item;
            if (equiped.Characters.Contains(player)) return false;

            alarmedEquipItem = (player, (EquipeableItem)item);

            if (equiped.IsEquip && !CheckHaveMoreItems(item.ID))
            {
                OnTryAlreadyEquiped?.Invoke();
                return false;
            }

            unequippedItems = UnequipWeaponSlot(player, alarmedEquipItem.item);
            Equip();
            return true;

            bool CheckHaveMoreItems(in int itemId)
            {
                return inventory.GetItemCount(itemId) > equiped.Characters.Count;
            }
        }

        private List<EquipeableItem> UnequipWeaponSlot(Character player, EquipeableItem item)
        {
            var equippedItems = player.Equipment.GetItems((int)item.GetEquipLocation());

            if (equippedItems == null || equippedItems.Count <= 0) return null;

            foreach (var equippedItem in equippedItems)
            {
                Unequip(player, equippedItem);
            }

            return equippedItems;
        }

        private void Equip()
        {
            var (player, equipeableItem) = alarmedEquipItem;
            equipeableItem.Equip(player);

            UpdateModel(player, equipeableItem);

            alarmedEquipItem = default;
        }

        private void InitInventory()
        {
            if (initialItems != null)
            {
                foreach (var itemData in initialItems)
                {
                    Add(itemData.Item.ID);

                    if (itemData.IsEquiped)
                    {
                        var item = inventory.GetItem(itemData.Item.ID);
                        TryEquip(itemData.Character, itemData.Item, out _);
                    }
                }
            }
        }

        private void UpdateModel(Character player, InventoryItem prefab)
        {
            if (prefab is EquipeableItem equipable && equipable != null)
            {
                ItemEquiper.EquipModification(ref player.Equipment, equipable);
            }
        }

        public bool Unequip(Character player, EquipeableItem item)
        {
            if (item is var unequipped && unequipped == null)
                return false;

            if (!item.Characters.Contains(player)) return false;

            item.Unequip(player);
            
            ItemEquiper.UnequipModification(ref player.Equipment, unequipped);
            return true;
        }

        public bool Add(int id)
        {
            return inventory.Add(id);
        }

        public bool Remove(int id)
        {
            var item = inventory.GetItem(id);

            if (item == null) return false;

            alarmedRemovedItem = (EquipeableItem)item;

            if (((EquipeableItem)item).IsEquip)
            {
                OnTryDeleteEquiped?.Invoke();
                return false;
            }

            RemoveAlarmedItem();

            return true;
        }

        private void RemoveAlarmedItem()
        {
            if (alarmedRemovedItem == null) return;

            inventory.Remove(alarmedRemovedItem.ID);

            alarmedRemovedItem = default;
        }

        public InventoryItem GetItem(int id)
        {
            return inventory.GetItem(id);
        }

        public List<InventoryItem> GetList(ItemType type)
        {
            return inventory.GetList(type);
        }

        public List<InventoryItem> GetEquipedItems(ItemType itemType, Character character)
        {
            var items = inventory.GetList(itemType);

            List<InventoryItem> equipedItems = new(); 

            foreach (var item in items)
            {
                var equiped = item as EquipeableItem;
                if (equiped.IsEquip && equiped.Characters.Contains(character))
                {
                    equipedItems.Add(equiped);
                }
            }

            return equipedItems;
        }

        public int GetItemCount(int id)
        {
            return inventory.GetItemCount(id);
        }
    }
}
