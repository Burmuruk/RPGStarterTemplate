using UnityEngine;
using Burmuruk.Tesis.Stats;

namespace Burmuruk.Tesis.Fighting
{
    public class Fighter : MonoBehaviour
    {
        [SerializeField] float detectionRadious = 8;

        StatsManager m_Stats;
        StatsManager m_targetStats;
        Inventary m_inventary;

        float m_minDistance;
        float m_damage;
        bool canAttack = true;

        private void Awake()
        {
            m_Stats = GetComponent<StatsManager>();
            m_targetStats = GetComponent<StatsManager>();
            m_inventary = FindObjectOfType<Inventary>();
        }

        private void FixedUpdate()
        {
            if (m_targetStats && canAttack)
            {
                BasicAttack();
            }
        }

        /// <summary>
        /// Executes a basic attack if it's close enough to the m_direction.
        /// </summary>
        private void BasicAttack()
        {
            if (Vector3.Distance(m_targetStats.transform.position, transform.position) < m_inventary.EquipedWeapon.MinDistance)
            {
                m_targetStats.HP -= m_Stats.Damage;
            }
        }

        
    }
}
