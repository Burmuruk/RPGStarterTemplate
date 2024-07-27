using Burmuruk.Tesis.Control;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
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
