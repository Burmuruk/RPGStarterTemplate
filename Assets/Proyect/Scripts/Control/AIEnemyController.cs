using Burmuruk.Tesis.Control;
using UnityEngine;

namespace Assets.Proyect.Scripts.Control
{
    public class AIEnemyController : Character
    {
        Formation formation;
        PlayerState playerState;
        AttackState attackState;

        public enum Formation
        {
            None,
            Follow,
            LockTarget,
            Free,
            Protect
        }

        public enum PlayerState
        {
            None,
            Combat,
            Patrol,
            Dead
        }

        public enum AttackState
        {
            None,
            BasicAttack,
            SpecialAttack,
        }

        public bool IsControlled { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public void DisableControll()
        {
            throw new System.NotImplementedException();
        }

        public void EnableControll()
        {
            throw new System.NotImplementedException();
        }

        protected override void DecisionManager()
        {
            base.DecisionManager();

            switch (formation)
            {
                case Formation.Follow:

                    attackState = AttackState.None;
                    break;

                case Formation.None:
                case Formation.Free:

                    if (isTargetFar || isTargetClose)
                    {
                        playerState = PlayerState.Combat;
                        attackState = AttackState.BasicAttack;
                    }
                    else
                    {
                        playerState = PlayerState.None;
                        attackState = AttackState.None;
                    }

                    break;

                case Formation.Protect:

                    if (isTargetFar || isTargetClose)
                    {
                        playerState = PlayerState.Combat;
                        attackState = AttackState.BasicAttack;
                    }
                    else
                    {
                        attackState = AttackState.None;
                    }

                    break;

                case Formation.LockTarget:
                    playerState = PlayerState.Combat;
                    attackState = AttackState.BasicAttack;
                    break;

                default:
                    break;
            }
        }

        protected override void ActionManager()
        {
            base.ActionManager();

            switch (playerState, attackState)
            {

                case (PlayerState.Combat, AttackState.BasicAttack):
                    if (isTargetFar || isTargetClose)
                    {
                        if (formation == Formation.Protect)
                        {
                            fighter.shouldGetClose = false;
                        }

                        fighter.BasicAttack();
                    }
                    break;
            }
        }
    }
}
