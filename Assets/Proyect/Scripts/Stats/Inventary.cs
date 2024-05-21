using Burmuruk.Tesis.Control;
using Burmuruk.Tesis.Fighting;
using Burmuruk.Tesis.Saving;
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

        Dictionary<ItemType, Dictionary<int, (ISaveableItem item, int count, int maxCount)>> m_owned = new()
        {
            { ItemType.Weapon, new Dictionary<int, (ISaveableItem item, int count, int maxCount)>() },
            { ItemType.Ability, new Dictionary<int, (ISaveableItem item, int count, int maxCount)>() },
            { ItemType.Modification, new Dictionary<int, (ISaveableItem item, int count, int maxCount)>() },
            { ItemType.Consumable, new Dictionary<int, (ISaveableItem item, int count, int maxCount)>() },
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
                        return (Weapon)GetItem(w.Type, weapon.item.GetSubType());
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

        public object CaptureState()
        {
            return m_owned;
        }
        public void RestoreState(object args)
        {
            m_owned = (Dictionary<ItemType, Dictionary<int, (ISaveableItem item, int count, int maxCount)>>)args;
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
                    generalList[subType].count + 1,
                    generalList[subType].maxCount + 1);
            }
            else
            {
                if (m_ItemsList.Get(type, subType) == null)
                    return false;

                generalList.Add(subType, (item, 1, 99));
            }

            return true;
        }

        public virtual bool Remove(ItemType type, int idx)
        {
            if (m_owned[type][idx].count > 1)
                m_owned[type][idx] = (m_owned[type][idx].item, m_owned[type][idx].count - 1, m_owned[type][idx].maxCount);
            else
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

        public int GetItemCount(ItemType type, int subType)
        {
            if (m_owned.ContainsKey(type))
            {
                if (m_owned[type].ContainsKey(subType))
                    return m_owned[type][subType].count;
                else
                    return 0;
            }

            return 0;
        }

        public int GetItemMaxCount(ItemType type, int subType)
        {
            return m_owned[type][subType].maxCount;
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
        public ItemType Type { get; }
        public int GetSubType();
        public string GetName();
        public string GetDescription();
    }

    public interface IEquipable
    {
        public BodyManager.BodyPart BodyPart { get; }
        public GameObject Prefab { get; }

        public void Equip(StatsManager stats);
        public void Remove(StatsManager stats);
    }

    public interface IUsable
    {
        void Use();
    }
}
