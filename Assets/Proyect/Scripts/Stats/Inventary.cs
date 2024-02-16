using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    class Inventary : MonoBehaviour
    {
        [Header("General Lists")]
        [SerializeField] List<Weapon> weapons;
        [SerializeField] List<Hability> habilites;
        [SerializeField] List<Modification> modifiers;
        [SerializeField] List<Modification> items;

        [Header("Status")]
        [SerializeField] int? m_equipedWeapon = 1;

        static List<Weapon> m_weapons;
        static List<Hability> m_habilites;
        static List<Modification> m_modifiers;
        static List<Modification> m_items;

        Dictionary<ItemType, List<(object, bool)>> m_owned = new()
        {
            { ItemType.Weapon, new List<(object, bool)>() },
            { ItemType.Hability, new List<(object, bool)>() },
            { ItemType.Modification, new List<(object, bool)>() },
            { ItemType.Consumable, new List<(object, bool)>() },
        };
        int habSlots;

        public event Action OnWeaponChanged;

        public Weapon EquipedWeapon
        {
            get
            {
                if (m_equipedWeapon.HasValue)
                {
                    return m_weapons[m_equipedWeapon.Value];
                }

                return null;
            }
        }

        private void Awake()
        {
            m_weapons ??= weapons;
            m_habilites ??= habilites;
            m_modifiers ??= modifiers;
        }

        private void Start()
        {
            
        }

        public void ChangeEquipedWeapon(int idx)
        {
            if (m_weapons != null && m_weapons[idx] != null)
            {
                m_equipedWeapon = idx;
                OnWeaponChanged?.Invoke();
            }
        }

        public void Add(ItemType type, object item)
        {
            (m_owned[type]??= new()).Add((item, false));
        }

        public void Drop(ItemType type, int idx)
        {
            m_owned[type].RemoveAt(idx);
        }

        public ReadOnlyCollection<(object, bool)> GetOwnedList(ItemType type)
        {
            return m_owned[type].AsReadOnly();
        }

        public object GetOwnedItem(ItemType type, int idx)
        {
            return m_owned[type][idx];
        }

        public void Equip(ItemType type)
        {

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
}
