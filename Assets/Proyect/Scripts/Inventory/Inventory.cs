using Burmuruk.Tesis.Saving;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Burmuruk.Tesis.Inventory
{
    public class Inventory : MonoBehaviour, IInventory, ISaveable
    {
        [Header("Status")]
        [SerializeField] ItemsList m_ItemsList;

        int id;
        bool isPersistentData = false;

        Dictionary<int, (InventoryItem item, int count)> m_owned = new();

        public event Action OnWeaponChanged;

        public int ID { get => id; }
        public bool IsPersistentData { get => isPersistentData; }

        private void Awake()
        {
            id = GetHashCode();
        }

        public object CaptureState()
        {
            return m_owned;
        }
        public void RestoreState(object args)
        {
            m_owned = (Dictionary<int, (InventoryItem item, int count)>)args;
        }

        public virtual bool Add(int id)
        {
            if (m_owned.ContainsKey(id))
            {
                if (m_owned[id].count < m_owned[id].item.Capacity)
                    return false;

                m_owned[id] = (
                    m_owned[id].item,
                    m_owned[id].count + 1);
            }
            else
            {
                if (m_ItemsList.Get(id) is var d && d == null)
                    return false;

                m_owned.Add(id, (d, 1));
            }

            return true;
        }

        public virtual bool Remove(int id)
        {
            if (m_owned[id].count > 1)
                m_owned[id] = (m_owned[id].item, m_owned[id].count - 1);
            else
                m_owned.Remove(id);

            return true;
        }

        public List<InventoryItem> GetOwnedList(ItemType type)
        {
            return (from inventoryItem in m_owned.Values
                    where inventoryItem.item.Type == type
                    select inventoryItem.item).ToList();
        }

        public InventoryItem GetOwnedItem(int id)
        {
            if (m_owned.ContainsKey(id))
            {
                return m_owned[id].item;
            }

            return null;
        }

        public int GetItemCount(int id)
        {
            if (m_owned.ContainsKey(id))
            {
                return m_owned[id].count;
            }

            return 0;
        }
    }

    public enum ItemType
    {
        None,
        Consumable,
        Ability,
        Modification,
        Weapon
    }

    public interface IUsable
    {
        void Use(object args, Action callback);
    }
}
