using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    public class Inventary : MonoBehaviour
    {
        [Header("Status")]
        [SerializeField] Weapon m_equipedWeapon;
        StatsManager stats;

        public struct EquipedItem
        {
            ISaveableItem item;
            bool isEquipped;
            int count;

            public ISaveableItem Item { get => item; }
            public int Count { get => count; }

            public EquipedItem(ISaveableItem item)
            {
                this.item = item;
                isEquipped = false;
                count = 0;
            }

            public EquipedItem(ISaveableItem item, int count) : this(item)
            {
                this.count = count;
            }

            public void Add(int amount = 1) => count += amount;
            public void Remove(int amount = 1) => count -= amount;

            public void Equip(bool value) => isEquipped = value;
        }

        Dictionary<ItemType, List<(EquipedItem item, int maxAmount)>> m_owned = new()
        {
            { ItemType.Weapon, new List<(EquipedItem item, int maxAmount)>() },
            { ItemType.Hability, new List<(EquipedItem item, int maxAmount)>() },
            { ItemType.Modification, new List<(EquipedItem item, int maxAmount)>() },
            { ItemType.Consumable, new List<(EquipedItem item, int maxAmount)>() },
        };

        public event Action OnWeaponChanged;

        public Weapon EquipedWeapon
        {
            get
            {
                return m_equipedWeapon;
            }
        }

        private void Awake()
        {
            //m_weapons ??= weapons;
            ////m_habilites ??= habilites;
            //m_modifiers ??= modifiers;
            stats = GetComponent<StatsManager>();
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (m_owned.Count <= 0 || m_owned[ItemType.Hability].Count <= 0)
                    return;

                ((IUsable)m_owned[ItemType.Hability][0].item.Item).Use();
            }
        }

        public void ChangeEquipedWeapon(Weapon weapon)
        {
            m_equipedWeapon = weapon;
            OnWeaponChanged?.Invoke();
        }

        public void Add(ItemType type, ISaveableItem item)
        {
            (m_owned[type]??= new()).Add((new(item), stats.GetSlots(type)));
        }

        public void Remove(ItemType type, int idx)
        {
            m_owned[type].RemoveAt(idx);
        }

        public List<EquipedItem> GetOwnedList(ItemType type)
        {
            return (from list in m_owned[type] select list.item).ToList();
        }

        public ISaveableItem GetOwnedItem(ItemType type, int idx)
        {
            return m_owned[type][idx].item.Item;
        }

        public void Equip(ItemType type, ISaveableItem item)
        {
            print("Equip");
            for (int i = 0; i < m_owned[type].Count; i++)
            {
                if (m_owned[type][i].Item1.Item == item)
                {
                    m_owned[type][i].Item1.Equip(true);
                    ((IEquipable)m_owned[type][i].Item1.Item).Equip(stats); 
                    return;
                }
            }
        }
    }

    public enum ItemType
    {
        None,
        Consumable,
        Hability,
        Modification,
        Weapon
    }

    public interface ISaveableItem
    {

    }

    public interface IEquipable
    {
        public void Equip(StatsManager stats);

        public void Remove(StatsManager stats);
    }

    public interface IUsable
    {
        void Use();
    }
}
