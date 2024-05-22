using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Weapon", order = 1)]
    public class Weapon : ScriptableObject, ISaveableItem, IEquipable
    {
        [Header("Information")]
        [SerializeField] string m_name;
        [SerializeField] string m_description;
        [SerializeField] WeaponType m_type;
        [SerializeField] BodyManager.BodyPart m_bodyPart;
        [SerializeField] GameObject prefab;
        [Space(), Header("Settings")]
        [SerializeField] int m_damage;
        [SerializeField] float m_rateDamage;
        [SerializeField] float m_minDistance;
        [SerializeField] float m_maxDistance;
        [SerializeField] float reloadTime;
        [SerializeField] int maxAmmo;

        public ItemType Type => ItemType.Weapon;
        public int Damage { get => m_damage; }
        public float DamageRate { get => m_rateDamage; }
        public float MinDistance { get => m_minDistance; }
        public float MaxDistance { get => m_maxDistance; }
        public BodyManager.BodyPart BodyPart { get => m_bodyPart; }
        public GameObject Prefab { get => prefab; }
        public int MaxAmmo { get => maxAmmo; }
        public int Ammo { get; private set; }
        public float ReloadTime { get => reloadTime; }

        public string GetName()
        {
            return m_name;
        }

        public string GetDescription()
        {
            return m_description;
        }

        int ISaveableItem.GetSubType()
        {
            return (int)m_type;
        }

        public void Equip(StatsManager stats)
        {
            throw new System.NotImplementedException();
        }

        public void Remove(StatsManager stats)
        {
            throw new System.NotImplementedException();
        }
    }

    public enum WeaponType
    {
        None,
        Sword,
        Gun
    }
}
