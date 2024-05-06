using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Weapon", order = 1)]
    public class Weapon : ScriptableObject, ISaveableItem
    {
        [SerializeField] WeaponType m_type;
        [SerializeField] int m_damage;
        [SerializeField] float m_rateDamage;
        [SerializeField] float m_minDistance;
        [SerializeField] string m_name;
        [SerializeField] string m_description;

        public ItemType Type => ItemType.Weapon;
        public int Damage { get => m_damage; }
        public float DamageRate { get => m_rateDamage; }
        public float MinDistance { get => m_minDistance; }

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
    }

    enum WeaponType
    {
        None,
        Sword,
        Gun
    }
}
