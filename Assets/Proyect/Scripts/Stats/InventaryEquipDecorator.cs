using Burmuruk.Tesis.Control;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

namespace Burmuruk.Tesis.Stats
{
    public class InventaryEquipDecorator : MonoBehaviour, IInventary
    {
        Inventary inventary;
        PlayerItemEquipper itemEquipper;

        (ItemType itemType, EquipedItem item) alarmedRemovedItem = default;
        (ItemType itemType, Character player, EquipedItem item) alarmedEquipItem = default;

        public event Action OnTryDeleteEquiped;
        public event Action OnTryAlreadyEquiped;

        public Weapon EquipedWeapon { get => inventary.EquipedWeapon; }

        public void SetInventary(Inventary inventary) => this.inventary = inventary;

        public void Equip(Character player, ItemType itemType, int type)
        {
            var item = inventary.GetOwnedItem(itemType, type);

            if (item != null) return;

            alarmedEquipItem = (itemType, player, (EquipedItem)item);

            if (((EquipedItem)item).IsEquip)
            {
                OnTryAlreadyEquiped?.Invoke();
            }

            EquipDirec();
        }

        public void EquipDirec()
        {
            var (itemType, player, equipedItem) = alarmedEquipItem;
            equipedItem.Equip(true, player);
            //equipedItem.OnUnequiped += Unequip;

            itemEquipper.Equip(itemType, equipedItem.GetSubType());

            alarmedEquipItem = default;
        }

        public void Unequip(Character player, EquipedItem item)
        {
            item.Equip(false, player);
            //item.OnUnequiped -= Unequip;

            itemEquipper.Unequip(item.ItemType, item.GetSubType());
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
            return GetOwnedList(type);
        }
    }
}
