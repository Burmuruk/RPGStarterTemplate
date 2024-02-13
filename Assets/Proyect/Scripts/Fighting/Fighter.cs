using UnityEngine;
using Burmuruk.Tesis.Stats;

namespace Burmuruk.Tesis.Fighting
{
    public class Fighter : MonoBehaviour
    {
        [SerializeField] float minDistance;

        StatsManager m_Stats;
        StatsManager m_targetStats;
        

        bool canAttack = true;

        private void Awake()
        {
            m_Stats = GetComponent<StatsManager>();
            m_targetStats = GetComponent<StatsManager>();
        }

        private void FixedUpdate()
        {
            if (m_targetStats && canAttack)
            {
                BasicAttack();
            }
        }

        /// <summary>
        /// Executes a basic attack if it's close enough to the target.
        /// </summary>
        private void BasicAttack()
        {
            if (Vector3.Distance(m_targetStats.transform.position, transform.position) < Weapon.MinDistance)
            {
                m_targetStats.HP -= m_Stats.Damage;
            }
        }
    }
}
