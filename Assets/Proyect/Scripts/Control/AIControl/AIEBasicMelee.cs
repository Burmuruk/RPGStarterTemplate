using UnityEngine;

namespace Burmuruk.Tesis.Control.AI
{
    public class AIEBasicMelee : AIEnemyController
    {
        Transform m_target;

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

        protected override void ChooseAttack()
        {
            if (isTargetFar || isTargetClose)
                playerAction = PlayerAction.Combat;
        }

        private void Attack()
        {
            m_target = GetNearestTarget(eyesPerceibed);
            if (m_target == null)
                m_target = GetNearestTarget(earsPerceibed);

            if (m_target == null) return;

            if (Vector3.Distance(m_target.position, transform.position)
                <= stats.MinDistance)
            {
                fighter.SetTarget(m_target);
                fighter.BasicAttack();
            }
        }

        protected override void MovementManager()
        {
            var dis = stats.MinDistance * .8f;

            switch (playerAction)
            {
                case PlayerAction.Following:

                    if (Vector3.Distance(leader.transform.position, transform.position) > dis)
                    {
                        Vector3 destiny = (transform.position - leader.transform.position).normalized * dis;
                        destiny += leader.transform.position;

                        mover.MoveTo(destiny);
                    }
                    break;

                case PlayerAction.Combat:

                    if (Vector3.Distance(m_target.position, transform.position) > dis)
                    {
                        Vector3 destiny = (transform.position - m_target.position).normalized * dis;
                        destiny += m_target.position;

                        mover.MoveTo(destiny);
                    }
                    break;
            }
        }
    }
}