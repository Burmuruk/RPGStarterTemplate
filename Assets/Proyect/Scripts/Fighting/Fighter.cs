using UnityEngine;
using Burmuruk.Tesis.Stats;
using Burmuruk.Tesis.Movement;
using MyDearAnima.Controll;
using Burmuruk.Utilities;

namespace Burmuruk.Tesis.Fighting
{
    public class Fighter : MonoBehaviour
    {
        [SerializeField] float detectionRadious = 8;

        StatsManager m_Stats;
        StatsManager m_targetStats;
        Inventary m_inventary;
        Movement.Movement m_movement;

        Transform m_target;
        CoolDownAction cdBasicAttack;
        public bool shouldGetClose = false;
        bool canAttack = true;

        private void Awake()
        {
            m_Stats = GetComponent<StatsManager>();
            m_targetStats = GetComponent<StatsManager>();
            m_inventary = FindObjectOfType<Inventary>();
        }

        private void Start()
        {
            cdBasicAttack = new CoolDownAction(m_Stats.DamageRate);
        }

        private void FixedUpdate()
        {
            if (m_targetStats && canAttack)
            {
                BasicAttack();
            }
        }

        public void SetTarget(Transform target)
        {
            m_target = target;
            m_targetStats = target.GetComponent<StatsManager>();
        }

        /// <summary>
        /// Executes a basic attack if it's close enough to the m_direction.
        /// </summary>
        public void BasicAttack()
        {
            if (Vector3.Distance(m_target.position, transform.position) < m_inventary.EquipedWeapon.MinDistance)
            {
                if (cdBasicAttack.CanUse)
                {
                    m_targetStats.HP -= m_Stats.Damage;
                    StartCoroutine(cdBasicAttack.CoolDown());
                }
            }
            else if (shouldGetClose)
            {
                m_movement.MoveTo(transform.position);
            }
        }
    }
}
