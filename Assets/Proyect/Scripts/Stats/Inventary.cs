using Burmuruk.Tesis.Control;
using Burmuruk.Tesis.Fighting;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    public class Inventary : MonoBehaviour, IInventary, ISaveable
    {
        [Header("Status")]
        [SerializeField] ItemsList m_ItemsList;
        StatsManager stats;

        int id;
        bool isPersistentData = false;

        Dictionary<ItemType, Dictionary<int, (ISaveableItem item, int maxAmount)>> m_owned = new()
        {
            { ItemType.Weapon, new Dictionary<int, (ISaveableItem item, int maxAmount)>() },
            { ItemType.Ability, new Dictionary<int, (ISaveableItem item, int maxAmount)>() },
            { ItemType.Modification, new Dictionary<int, (ISaveableItem item, int maxAmount)>() },
            { ItemType.Consumable, new Dictionary<int, (ISaveableItem item, int maxAmount)>() },
        };


        public event Action OnWeaponChanged;

        public int ID { get => id; }
        public bool IsPersistentData { get => isPersistentData; }
        public Weapon EquipedWeapon
        {
            get
            {
                foreach (var weapon in m_owned[ItemType.Weapon].Values)
                {
                    if (((EquipedItem)weapon.item) is var w && w.IsEquip)
                    {
                        return (Weapon)GetItem(w.ItemType, weapon.item.GetSubType());
                    }
                }

                return null;
            }
        }

        private void Awake()
        {
            //m_weapons ??= weapons;
            ////m_habilites ??= abilities;
            //m_modifiers ??= modifiers;
            id = GetHashCode();
            stats = GetComponent<StatsManager>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (m_owned.Count <= 0 || m_owned[ItemType.Ability].Count <= 0)
                    return;

                var type = (AbilityType)(m_owned[ItemType.Ability][0].item.GetSubType());
                ((IUsable)m_ItemsList.GetAbility(type)).Use();
                
            }
        }

        public object Save()
        {
            return m_owned;
        }
        public void Load(object args)
        {
            m_owned = (Dictionary<ItemType, Dictionary<int, (ISaveableItem item, int maxAmount)>>)args;
        }

        public virtual bool Add(ItemType type, ISaveableItem item)
        {
            //if (GetItem(type, item.GetSubType()) == null) 
            //    return false;

            var generalList = (m_owned[type] ??= new());

            int subType = item.GetSubType();

            if (generalList.ContainsKey(subType))
            {
                generalList[subType] = (
                    generalList[subType].item,
                    generalList[subType].maxAmount + 1);
            }
            else
            {
                generalList.Add(subType, (item, 99));
            }

            return true;
        }

        public virtual bool Remove(ItemType type, int idx)
        {
            m_owned[type].Remove(idx);

            return true;
        }

        public List<ISaveableItem> GetOwnedList(ItemType type)
        {
            return (from list in m_owned[type] select list.Value.item).ToList();
        }

        public ISaveableItem GetOwnedItem(ItemType type, int idx)
        {
            if (m_owned[type].ContainsKey(idx))
            {
                return m_owned[type][idx].item;
            }

            return null;
        }

        public List<ISaveableItem> GetList(ItemType type)
        {
            List<ISaveableItem> realList = new();

            foreach (var item in GetOwnedList(type))
            {
                realList.Add(m_ItemsList.Get(type, item.GetSubType()));
            }

            return realList;
        }

        public ISaveableItem GetItem(ItemType type, int subType)
        {
            if (GetOwnedItem(type, subType) == null) 
                return null;

            return m_ItemsList.Get(type, subType);
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

    public interface ISaveableItem
    {
        public int GetSubType();
        public string GetName();
        public string GetDescription();
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
