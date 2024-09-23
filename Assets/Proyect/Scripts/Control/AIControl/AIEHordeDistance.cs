using Burmuruk.Tesis.Stats;
using Burmuruk.Utilities;
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
        
        CoolDownAction cdHorde;
        CoolDownAction cdHordeInkoke;
        List<AIEnemyController> hordeMembers = new();
        bool troopsDeployed;

        protected override void Awake()
        {
            base.Awake();
            Initialize();
        }

        public override void SetStats(BasicStats stats)
        {
            Initialize();

            base.SetStats(stats);
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
            //if (IsTargetFar && !IsTargetClose && !troopsDeployed && cdHorde.CanUse)
            //{
            //    if (cdHordeInkoke.CanUse)
            //        StartCoroutine(cdHordeInkoke.CoolDown());

            //    Invoke("EnableHorde", hordeInvokeTime * .8f);
            //    playerAction = PlayerAction.None;
            //    return;
            //}

            m_target = GetNearestTarget(eyesPerceibed)?.GetComponent<Character>();
            if (m_target == null)
                m_target = GetNearestTarget(earsPerceibed)?.GetComponent<Character>();

            if (m_target == null) return;

            if (isTargetFar && !IsTargetClose)
            {
                fighter.SetTarget(m_target.transform);
                fighter.BasicAttack();
                EnableHorde();
                hordeMembers.ForEach(enemy => enemy.SetTarget(m_target));
                hordeMembers.ForEach(enemy => enemy.SetOrder(LeaderOrder.Attack));
            }
        }

        protected override void MovementManager()
        {
            switch (playerAction)
            {
                case PlayerAction.Combat:

                    if (troopsDeployed)
                    {
                        if (!m_target) return;

                        var dis = stats.MinDistance * .8f;

                        if (Vector3.Distance(m_target.transform.position, transform.position) > dis)
                        {
                            Vector3 destiny = (transform.position - m_target.transform.position).normalized * dis;
                            destiny += m_target.transform.position;

                            mover.MoveTo(destiny);
                        }
                    }
                    else if (IsTargetClose)
                    {
                        mover.Flee(m_target.transform.position);
                    }
                    else if (isTargetFar && !IsTargetClose)
                    {

                    }

                    break;
            }
        }

        private void Initialize()
        {
            cdHorde = new CoolDownAction(coolDownHorde);
            cdHordeInkoke = new CoolDownAction(hordeInvokeTime);
            health.OnDamaged += (_) => DelayHorde();
        }

        private void EnableHorde()
        {
            troopsDeployed = true;
            horde.SetActive(true);

            for (int i = 0; i < horde.transform.childCount; i++)
            {
                var enemy = horde.transform.GetChild(i).GetComponent<AIEnemyController>();

                hordeMembers.Add(enemy);
                enemy.SetLeader(this);
                //enemy.SetOrder(LeaderOrder.Follow);
            }
        }

        protected override void Dead()
        {
            ReleaseHorde();

            base.Dead();
        }

        private void ReleaseHorde()
        {
            hordeMembers.ForEach(enemy => enemy.SetOrder(LeaderOrder.None));
            gameObject.SetActive(false);
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