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

        public int Damage { get => m_damage; }
        public float DamageRate { get => m_rateDamage; }
        public float MinDistance { get => m_minDistance; }

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
