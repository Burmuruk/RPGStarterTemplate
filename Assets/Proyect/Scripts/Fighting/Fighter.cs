using Burmuruk.Tesis.Control;
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
        IInventary m_inventary;
        Movement.Movement m_movement;
        AbilitiesManager habManager;

        Transform m_target;
        CoolDownAction cdBasicAttack;
        public bool shouldGetClose = false;
        bool canAttack = true;

        private void Awake()
        {
            m_Stats = GetComponent<StatsManager>();
            m_targetStats = GetComponent<StatsManager>();
            m_inventary = GetComponent<IInventary>();
        }

        private void Start()
        {
            //var decorator = m_inventary as InventaryEquipDecorator;

            //if (decorator != null)
            //{
            //    decorator.Add(ItemType.Weapon, 0);
            //    decorator.Equip(GetComponent<Character>(), ItemType.Weapon, 0);
            //}

            //if (m_Stats.DamageRate == 0) return;
            cdBasicAttack = new CoolDownAction(m_Stats.DamageRate);
        }

        private void FixedUpdate()
        {
            if (m_targetStats && canAttack)
            {
                BasicAttack();
            }

            if (m_Stats.DamageRate != 0)
                cdBasicAttack = new CoolDownAction(m_Stats.DamageRate);
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

            if (cdBasicAttack.CanUse)
            {
                m_targetStats.ApplyDamage(m_Stats.Damage);
                StartCoroutine(cdBasicAttack.CoolDown());
            }
        }

        public void SpecialAttack(AbilityType type)
        {
            var habilities = m_inventary.GetOwnedList(ItemType.Ability);

            foreach (var hability in habilities)
            {
                if (hability.GetSubType() == (int)type)
                {
                    var args = GetSpecialAttackArgs(type);
                    AbilitiesManager.habilitiesList[type]?.Invoke(args);
                    return;
                }
            }
        }

        private object GetSpecialAttackArgs(AbilityType type) =>
            type switch
            {
                AbilityType.Dash => m_movement.CurDirection,
                AbilityType.StealHealth => m_target,
                _ => null
            };
    }
}
