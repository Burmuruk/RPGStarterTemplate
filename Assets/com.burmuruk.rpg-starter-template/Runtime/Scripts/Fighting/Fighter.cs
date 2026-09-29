using Burmuruk.RPGStarterTemplate.Control;
using Burmuruk.RPGStarterTemplate.Inventory;
using Burmuruk.RPGStarterTemplate.Stats;
using Burmuruk.Utilities;
using System;
using System.Collections;
using UnityEngine;

namespace Burmuruk.RPGStarterTemplate.Combat
{
    public class Fighter : MonoBehaviour
    {
        [SerializeField] float detectionRadious = 8;

        Func<BasicStats> m_Stats;
        Health m_targetHealth;
        InventoryEquipDecorator m_inventory;
        Movement.Movement m_movement;
        AbilitiesManager habManager;

        Transform m_target;
        CoolDownAction cdBasicAttack;
        Coroutine basicAttackC;
        Coroutine autoBACoroutine;
        public bool shouldGetClose = false;
        bool canAttack = true;
        bool inAutoAttack = false;

        BasicStats Stats { get => m_Stats.Invoke(); }
        public int Damage
        {
            get
            {
                return Stats.damage;
            }
        }

        enum Satate
        {
            None,
            Paused,
            Working,
        }

        private void FixedUpdate()
        {
            if (m_targetHealth && canAttack)
            {
                BasicAttack();
            }

            //if (m_Stats.DamageRate != 0)
            //    cdBasicAttack = new CoolDownAction(m_Stats.DamageRate);
        }

        public void Initilize(InventoryEquipDecorator inventory, Func<BasicStats> stats)
        {
            m_inventory = inventory;
            m_Stats = stats;

            float rate = m_Stats.Invoke().damageRate;
            cdBasicAttack = new CoolDownAction(in rate);
            inAutoAttack = false;
        }

        public void Pause(bool shouldPause)
        {
            canAttack = !shouldPause;
        }

        public void ResetCombat()
        {
            StopAllCoroutines();
            cdBasicAttack?.Cancel();
            autoBACoroutine = null;
            inAutoAttack = false;
            SetTarget(null);
            canAttack = true;
        }

        public void SetTarget(Transform target)
        {
            if (m_targetHealth != null)
                m_targetHealth.OnDied -= RemoveTarget;

            m_target = target;
            m_targetHealth = target != null ? target.GetComponent<Health>() : null;

            if (m_targetHealth != null)
                m_targetHealth.OnDied += RemoveTarget;
        }
        public void RemoveTarget(Transform target)
        {
            if (m_target != target) return;
            SetTarget(null);
        }

        /// <summary>
        /// Executes a basic attack if it's close enough to the m_direction.
        /// </summary>
        public void BasicAttack()
        {
            if (!canAttack || !isActiveAndEnabled || !m_target || !m_target.gameObject.activeInHierarchy ||
                m_targetHealth == null || !m_targetHealth.IsAlive || cdBasicAttack == null) return;

            if (cdBasicAttack.CanUse)
            {
                if (Vector3.Distance(m_target.position, transform.position) > Stats.minDistance)
                    return;

                EquipableItem equipable = m_inventory?.Equipped[(int)Inventory.EquipmentType.WeaponR];
                cdBasicAttack.Restart();
                StartCoroutine(cdBasicAttack.CoolDown());
                m_targetHealth.ApplyDamage(Stats.damage);

                if (equipable is Weapon weapon && weapon.TryGetBuff(out BuffData? buff))
                {
                    if (buff.HasValue && BuffsManager.Instance != null)
                        BuffsManager.Instance.AddBuff(transform.GetComponent<Control.Character>(), buff.Value);
                }
            }
        }

        public void StartAutoBasicAttack(bool start)
        {
            if (start)
            {
                if (inAutoAttack) return;

                autoBACoroutine = StartCoroutine(AutoBasicAttackCoroutine()); 
            }
            else
            {
                if (autoBACoroutine != null)
                    StopCoroutine(autoBACoroutine);

                autoBACoroutine = null;
                inAutoAttack = false;
            }
        }

        public void SpecialAttack(AbilityType type)
        {
            var habilities = m_inventory.GetList(ItemType.Ability);

            foreach (var hability in habilities)
            {
                if ((AbilityType)hability.GetSubType() == type)
                {
                    var args = GetSpecialAttackArgs(type);
                    //AbilitiesManager.habilitiesList[modifiableStat]?.Invoke(args);
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

        private IEnumerator AutoBasicAttackCoroutine()
        {
            inAutoAttack = true;

            while (m_target != null && m_targetHealth != null && m_targetHealth.IsAlive &&
                m_target.gameObject.activeInHierarchy)
            {
                BasicAttack();
                yield return null;
            }

            inAutoAttack = false;
            autoBACoroutine = null;
        }
    }
}
