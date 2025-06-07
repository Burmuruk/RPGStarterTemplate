using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using Burmuruk.Utilities;
using System;
using UnityEngine;

namespace Burmuruk.Tesis.Combat
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
        public bool shouldGetClose = false;
        bool canAttack = true;

        BasicStats Stats { get => m_Stats.Invoke(); }

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

            //m_inventory.Equipped.OnEquipmentChanged += (id) =>
            //{
            //    if (id == (int)EquipmentType.WeaponR)
            //    {
            //        CacheWeapon();
            //    }
            //};

            float rate = m_Stats.Invoke().damageRate;
            cdBasicAttack = new CoolDownAction(in rate);
        }

        public void Pause(bool shouldPause)
        {
            canAttack = shouldPause;
        }

        public void SetTarget(Transform target)
        {
            m_target = target;
            m_targetHealth = target.GetComponent<Health>();
        }

        /// <summary>
        /// Executes a basic attack if it's close enough to the m_direction.
        /// </summary>
        public void BasicAttack()
        {
            if (!m_target) return;

            if (cdBasicAttack.CanUse)
            {
                if (Vector3.Distance(m_target.position, transform.position) > Stats.minDistance)
                    return;

                m_targetHealth.ApplyDamage(Stats.damage);

                EquipeableItem weapon = m_inventory.Equipped[(int)Inventory.EquipmentType.WeaponR];

                if (weapon != null && (weapon as Weapon).TryGetBuff(out BuffData? buff))
                {
                    if (buff.HasValue)
                        BuffsManager.Instance.AddBuff(transform.GetComponent<Control.Character>(), buff.Value, () => m_targetHealth.ApplyDamage(Stats.damage));
                }

                if (!gameObject.activeSelf) return;

                StartCoroutine(cdBasicAttack.CoolDown());
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
    }
}
