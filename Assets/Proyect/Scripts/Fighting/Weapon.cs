using Burmuruk.Tesis.Control;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System.Linq;
using UnityEngine;

namespace Burmuruk.Tesis.Combat
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Weapon", order = 1)]
    public class Weapon : EquipeableItem
    {
        [Header("Equipment")]
        [SerializeField] EquipmentType m_bodyPart;
        [Space(), Header("Settings")]
        [SerializeField] WeaponType weaponType;
        [SerializeField] int m_damage;
        [SerializeField] float m_rateDamage;
        [SerializeField] float m_minDistance;
        [SerializeField] float m_maxDistance;
        [SerializeField] float reloadTime;
        [SerializeField] int maxAmmo;
        [Space(), Header("Buffs")]
        [SerializeField] BuffData[] m_buffsData;
        [Space(), Header("Modifications")]
        [SerializeField] Equipment equipment;

        public int Damage { get => m_damage; }
        public float DamageRate { get => m_rateDamage; }
        public float MinDistance { get => m_minDistance; }
        public float MaxDistance { get => m_maxDistance; }
        public EquipmentType BodyPart { get => m_bodyPart; }
        public int MaxAmmo { get => maxAmmo; }
        public int Ammo { get; private set; }
        public float ReloadTime { get => reloadTime; }
        public BuffData[] BuffsData { get => m_buffsData; }

        public bool TryGetBuff(out BuffData? buffData)
        {
            buffData = null;

            if (m_buffsData == null || m_buffsData.Length == 0) return false;

            int idx = Random.Range(0, m_buffsData.Length);

            if (m_buffsData[idx].probability == 1)
            {
                buffData = m_buffsData[idx];
                return true;
            }

            float probability = Random.Range(0, 1.0f);

            if (probability <= m_buffsData[idx].probability)
            {
                buffData = m_buffsData[idx];
                return true;
            }
            else return false;
        }

        public override object GetEquipLocation()
        {
            return m_bodyPart;
        }

        public override void Equip(Character character)
        {
            base.Equip(character);
            ModsList.AddModification(character, ModifiableStat.BaseDamage, m_damage);
            ModsList.AddModification(character, ModifiableStat.GunFireRate, m_rateDamage);
            ModsList.AddModification(character, ModifiableStat.MinDistance, m_minDistance);
        }

        public override void Unequip(Character character)
        {
            base.Unequip(character);

            ModsList.RemoveModification(character, ModifiableStat.BaseDamage, m_damage);
            ModsList.RemoveModification(character, ModifiableStat.GunFireRate, m_rateDamage);
            ModsList.RemoveModification(character, ModifiableStat.MinDistance, m_minDistance);
        }

        public override void Populate(string name, string description, ItemType type, 
            Sprite sprite, Pickup pickup, int capacity, object args)
        {
            base.Populate(name, description, type, sprite, pickup, capacity, args);

            var values = ((int damage, float rateDamage, float minDistance, float maxDistance,
                float reloadTime, int maxAmmo, BuffData[] data))args;

            (m_damage, m_rateDamage, m_minDistance, m_maxDistance, reloadTime, maxAmmo, m_buffsData) = values;
        }

        private void EquipMod()
        {

        }

        private void UnequipMod()
        {

        }
    }

    public enum WeaponType
    {
        None,
        Sword,
        Gun
    }
}
