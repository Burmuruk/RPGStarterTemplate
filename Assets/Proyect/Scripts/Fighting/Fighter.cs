using Burmuruk.Tesis.Stats;
using Burmuruk.Utilities;
using UnityEngine;

namespace Burmuruk.Tesis.Fighting
{
    public class Fighter : MonoBehaviour
    {
        [SerializeField] float detectionRadious = 8;

        StatsManager m_Stats;
        StatsManager m_targetStats;
        Inventary m_inventary;
        Movement.Movement m_movement;
        HabilitiesManager habManager;

        Transform m_target;
        CoolDownAction cdBasicAttack;
        public bool shouldGetClose = false;
        bool canAttack = true;

        private void Awake()
        {
            m_Stats = GetComponent<StatsManager>();
            m_targetStats = GetComponent<StatsManager>();
            m_inventary = GetComponent<Inventary>();
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
            if (!m_target) return;

            if (Vector3.Distance(m_target.position, transform.position) < m_inventary.EquipedWeapon.MinDistance)
            {
                if (cdBasicAttack.CanUse)
                {
                    m_targetStats.ApplyDamage(m_Stats.Damage);
                    StartCoroutine(cdBasicAttack.CoolDown());
                }
            }
            else if (shouldGetClose)
            {
                m_movement.MoveTo(transform.position);
            }
        }

        public void SpecialAttack(HabilityType type)
        {
            var habilities = m_inventary.GetOwnedList(ItemType.Hability);

            foreach (var hability in habilities)
            {
                if (((Hability)hability.Item).Type == type)
                {
                    var args = GetSpecialAttackArgs(type);
                    HabilitiesManager.habilitiesList[type]?.Invoke(args);
                    return;
                }
            }
        }

        private object GetSpecialAttackArgs(HabilityType type) =>
            type switch
            {
                HabilityType.Dash => m_movement.CurDirection,
                HabilityType.StealHealth => m_target,
                _ => null
            };
    }
}
