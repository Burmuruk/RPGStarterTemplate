using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using Burmuruk.Utilities;
using UnityEngine;

namespace Burmuruk.Tesis.Combat
{
    public class Fighter : MonoBehaviour
    {
        [SerializeField] float detectionRadious = 8;

        Stats.BasicStats m_Stats;
        //StatsManager m_targetStats;
        InventoryEquipDecorator m_inventory;
        Movement.Movement m_movement;
        AbilitiesManager habManager;

        Transform m_target;
        CoolDownAction cdBasicAttack;
        Weapon weapon;
        public bool shouldGetClose = false;
        bool canAttack = true;

        private void FixedUpdate()
        {
            if (m_targetStats && canAttack)
            {
                BasicAttack();
            }

            //if (m_Stats.DamageRate != 0)
            //    cdBasicAttack = new CoolDownAction(m_Stats.DamageRate);
        }

        public void Initilize(InventoryEquipDecorator inventory, ref BasicStats stats)
        {
            m_inventory = inventory;
            m_Stats = stats;

            m_inventory.Equipped.OnEquipmentChanged += (id) =>
            {
                if (id == (int)BodyPart.RArm)
                {
                    CacheWeapon();
                }
            };

            cdBasicAttack = new CoolDownAction(weapon.DamageRate);
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
                print(transform.name + " can use");
                m_targetStats.ApplyDamage(m_Stats.Damage);
                StartCoroutine(cdBasicAttack.CoolDown());
            }
        }

        public void SpecialAttack(AbilityType type)
        {
            var habilities = m_inventory.GetOwnedList(ItemType.Ability);

            foreach (var hability in habilities)
            {
                if ((AbilityType)hability.GetSubType() == type)
                {
                    var args = GetSpecialAttackArgs(type);
                    //AbilitiesManager.habilitiesList[subType]?.Invoke(args);
                    return;
                }
            }
        }

        private void CacheWeapon()
        {
            int id = m_inventory.Equipped[(int)BodyPart.RArm];
            weapon = (Weapon)m_inventory.GetOwnedItem(id);
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
