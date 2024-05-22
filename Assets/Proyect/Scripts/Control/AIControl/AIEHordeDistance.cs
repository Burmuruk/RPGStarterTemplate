using Burmuruk.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Control.AI
{
    public class AIEHordeDistance : AIEnemyController
    {
        [SerializeField] GameObject horde;
        [SerializeField] float coolDownHorde;
        [SerializeField] float hordeInvokeTime;
        //[SerializeField] List<AIEnemyController> horde = new();
        Transform m_target;
        CoolDownAction cdHorde;
        CoolDownAction cdHordeInkoke;
        List<AIEnemyController> hordeMembers = new();
        bool troopsDeployed;

        protected override void Start()
        {
            base.Start();

            cdHorde = new CoolDownAction(coolDownHorde);
            cdHordeInkoke = new CoolDownAction(hordeInvokeTime);
            stats.OnDamage += (_) => DelayHorde();
            stats.OnDied += ReleaseHorde;
        }

        protected override void DecisionManager()
        {
            if (!cdHordeInkoke.CanUse) return;

            base.DecisionManager();
        }

        protected override void FindEnemies()
        {
            if (!IsTargetClose && !IsTargetFar) return;

            playerAction = PlayerAction.Combat;
            attackState = AttackState.BasicAttack;
        }

        protected override void ActionManager()
        {
            base.ActionManager();

            switch (playerAction)
            {
                case PlayerAction.Combat:
                    Attack();
                    break;

                default:
                    break;
            }
        }

        private void Attack()
        {
            if (IsTargetFar && !IsTargetClose && !troopsDeployed && cdHorde.CanUse)
            {
                if (cdHordeInkoke.CanUse)
                    StartCoroutine(cdHordeInkoke.CoolDown());

                Invoke("SetHordePositions", hordeInvokeTime * .8f);
                playerAction = PlayerAction.None;
                return;
            }

            m_target = GetNearestTarget(eyesPerceibed);
            if (m_target == null)
                m_target = GetNearestTarget(earsPerceibed);

            if (Vector3.Distance(m_target.position, transform.position)
                <= Inventary.EquipedWeapon.MinDistance)
            {
                fighter.SetTarget(m_target);
                fighter.BasicAttack();
                hordeMembers.ForEach(enemy => enemy.SetOrder(LeaderOrder.Attack));
            }
        }

        protected override void MovementManager()
        {
            switch (playerAction)
            {
                case PlayerAction.Combat:

                    var dis = Inventary.EquipedWeapon.MinDistance * .8f;

                    if (Vector3.Distance(m_target.position, transform.position) > dis)
                    {
                        Vector3 destiny = (transform.position - m_target.position).normalized * dis;
                        destiny += m_target.position;

                        mover.MoveTo(destiny);
                    }
                break;
            }
        }

        private void SetHordePositions()
        {
            troopsDeployed = true;
            horde.SetActive(true);

            for (int i = 0; i < horde.transform.childCount; i++)
            {
                var enemy = horde.transform.GetChild(i).GetComponent<AIEnemyController>();

                hordeMembers.Add(enemy);
                enemy.SetLeader(this);
                enemy.SetOrder(LeaderOrder.Follow);
            }
        }

        private void ReleaseHorde()
        {
            hordeMembers.ForEach(enemy => enemy.SetOrder(LeaderOrder.None));
        }

        private void DelayHorde()
        {
            if (troopsDeployed) return;

            if (!cdHorde.CanUse)
            {
                StopCoroutine(cdHorde.CoolDown());
                cdHorde.Restart();
            }

            StartCoroutine(cdHorde.CoolDown());
        }
    }
}