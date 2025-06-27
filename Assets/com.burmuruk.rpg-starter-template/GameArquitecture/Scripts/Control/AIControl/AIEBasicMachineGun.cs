using UnityEngine;

namespace Burmuruk.Tesis.Control.AI
{
    public class AIEBasicMachineGun : AIEnemyController
    {
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
            m_target = GetNearestTarget(eyesPerceibed);
            if (m_target == null)
                GetNearestTarget(earsPerceibed);

            if (Vector3.Distance(m_target.position, transform.position)
                <= stats.minDistance)
            {
                fighter.SetTarget(m_target);
                fighter.BasicAttack();
            }
        }

        protected override void MovementManager()
        {
            switch (playerAction)
            {
                case PlayerAction.Combat:

                    var dis = stats.minDistance * .8f;

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