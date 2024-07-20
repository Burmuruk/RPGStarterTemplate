using Burmuruk.Tesis.Inventory;
using UnityEngine;

namespace Burmuruk.Tesis.Combat
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Weapon", order = 1)]
    public class Weapon : EquipeableItem
    {
        [Header("Information")]
        [SerializeField] WeaponType m_type;
        [SerializeField] BodyPart m_bodyPart;
        [Space(), Header("Settings")]
        [SerializeField] int m_damage;
        [SerializeField] float m_rateDamage;
        [SerializeField] float m_minDistance;
        [SerializeField] float m_maxDistance;
        [SerializeField] float reloadTime;
        [SerializeField] int maxAmmo;

        public int Damage { get => m_damage; }
        public float DamageRate { get => m_rateDamage; }
        public float MinDistance { get => m_minDistance; }
        public float MaxDistance { get => m_maxDistance; }
        public BodyPart BodyPart { get => m_bodyPart; }
        public int MaxAmmo { get => maxAmmo; }
        public int Ammo { get; private set; }
        public float ReloadTime { get => reloadTime; }

        public override object GetEquipLocation()
        {
            return m_type;
        }
    }

    public enum WeaponType
    {
        None,
        Sword,
        Gun
    }
}
