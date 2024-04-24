using Burmuruk.Tesis.Fighting;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Inventary", order = 1)]
    class ItemsList : ScriptableObject
    {
        [Header("General Lists")]
        [SerializeField] List<Weapon> weapons;
        [SerializeField] List<Modification> modifiers;
        [SerializeField] List<Pickable> items;
        [SerializeField] List<Ability> abilities;
        [SerializeField] bool Initialized;

        Dictionary<PickableType, Pickable> m_items;
        Dictionary<WeaponType, Weapon> m_weapon;
        Dictionary<AbilityType, Ability> m_ability;
        Dictionary<ModificationType, Modification> m_modifications;

        public List<Pickable> Items { get => items; }
        public List<Modification> Modifiers { get => modifiers; }
        public List<Ability> Abilities { get => abilities; }

        public Ability GetAbility(AbilityType type)
        {
            if (!Initialized) Initialize();

            return m_ability[type];
        }

        public Pickable GetItem(PickableType type)
        {
            if (!Initialized) Initialize();

            return m_items[type];
        }

        public Modification GetItem(ModificationType type)
        {
            if (!Initialized) Initialize();

            return m_modifications[type];
        }

        public Weapon GetItem(WeaponType type)
        {
            if (!Initialized) Initialize();

            return m_weapon[type];
        }

        public ISaveableItem Get(ItemType item, int type)
        {
            if (!Initialized) Initialize();

            return item switch
            {
                ItemType.Weapon => m_weapon[(WeaponType)type],
                ItemType.Ability => m_ability[(AbilityType)type],
                ItemType.Modification => m_modifications[(ModificationType)type],
                ItemType.Consumable => m_items[(PickableType)type],
                _ => null
            };
        }

        private void Initialize()
        {
            m_weapon = new Dictionary<WeaponType, Weapon>();

            foreach (var weapon in weapons)
            {
                var key = (WeaponType)((ISaveableItem)weapon).GetSubType();
                if (!m_weapon.ContainsKey(key))
                    m_weapon.Add(key, weapon);
            }

            m_items = new Dictionary<PickableType, Pickable>();

            foreach (var pickable in items)
            {
                var key = (PickableType)((ISaveableItem)pickable).GetSubType();
                if (!m_items.ContainsKey(key))
                    m_items.Add(key, pickable);
            }

            m_modifications = new Dictionary<ModificationType, Modification>();

            foreach (var modifier in modifiers)
            {
                var key = (ModificationType)((ISaveableItem)modifier).GetSubType();
                if (!m_modifications.ContainsKey(key))
                    m_modifications.Add(key, modifier);
            }

            m_ability = new Dictionary<AbilityType, Ability>();

            foreach (var ability in abilities)
            {
                var key = (AbilityType)((ISaveableItem)ability).GetSubType();
                if (!m_ability.ContainsKey(key))
                    m_ability.Add(key, ability);
            }
        }
    }
}
