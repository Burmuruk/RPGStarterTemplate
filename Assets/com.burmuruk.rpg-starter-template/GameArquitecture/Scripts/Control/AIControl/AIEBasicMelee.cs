using UnityEngine;

namespace Burmuruk.Tesis.Control.AI
{
    public class AIEBasicMelee : AIEnemyController
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

        protected override void ChooseAttack()
        {
            if (isTargetFar || isTargetClose)
                playerAction = PlayerAction.Combat;
        }

        private void Attack()
        {
            if (IsTargetFar)
            {
                Target = GetNearestTarget(eyesPerceibed);
            }
            else if (IsTargetClose)
            {
                Target = GetNearestTarget(earsPerceibed);
            }
            else if (Target == null) return;

            if (Vector3.Distance(Target.position, transform.position)
                <= stats.minDistance)
            {
                fighter.SetTarget(Target);
                fighter.BasicAttack();
            }
        }

        protected override void MovementManager()
        {
            var dis = stats.minDistance * .8f;

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

                    if (Vector3.Distance(Target.position, transform.position) > dis)
                    {
                        Vector3 destiny = (transform.position - Target.position).normalized * dis;
                        destiny += Target.position;

                        mover.MoveTo(destiny);
                    }
                    break;
            }
        }
    }
}