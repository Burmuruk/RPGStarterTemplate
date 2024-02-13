using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/Weapon", order = 1)]
    class Weapon : ScriptableObject
    {
        [SerializeField] float m_damage;
        [SerializeField] float m_rateDamage;
        [SerializeField] float m_minDistance;

        public float Damage { get => m_damage; }
        public float RateDamage { get => m_rateDamage; }
        public float MinDistance { get => m_minDistance; }
    }
}
