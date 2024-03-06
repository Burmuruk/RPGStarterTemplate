using System;
using UnityEngine;

namespace Burmuruk.Tesis.Control
{
    public class AIGuildMember : Character, IPlayable
    {
        Formation formation;
        PlayerState playerState;
        Character mainPlayer;
        PlayerDistance playerDistance;
        AttackState attackState;

        const float closeDistance = 3;
        const float freeDistance = 6;
        const float farDistance = 9;

        public event Action OnCombatStarted;

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
            FollowPlayer,
            Patrol,
            Teleporting,
            Dead
        }

        public enum PlayerDistance
        {
            None,
            Close,
            Free,
            Far,
            FarAway
        }

        public enum AttackState
        {
            None,
            BasicAttack,
            SpecialAttack,
        }

        public enum MovementState
        {
            None,

        }

        public bool IsControlled { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            playerDistance = Vector3.Distance(mainPlayer.transform.position, transform.position) switch
            {
                < closeDistance => PlayerDistance.Close,
                < freeDistance => PlayerDistance.Free,
                < farDistance => PlayerDistance.Far,
                _ => PlayerDistance.FarAway
            };
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

            if (playerDistance == PlayerDistance.FarAway)
            {
                playerState = PlayerState.Teleporting;
                return;
            }

            switch (formation)
            {
                case Formation.Follow:

                    playerState = PlayerState.FollowPlayer;
                    attackState = AttackState.None;
                    break;

                case Formation.None:
                case Formation.Free:

                    if (playerDistance == PlayerDistance.Free || playerDistance == PlayerDistance.Close)
                    {
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
                    }

                    break;

                case Formation.Protect:

                    if (playerDistance == PlayerDistance.Free || playerDistance == PlayerDistance.Close)
                    {
                        if (isTargetFar || isTargetClose)
                        {
                            playerState = PlayerState.Combat;
                            attackState = AttackState.BasicAttack;
                        }
                    }
                    else if (playerDistance == PlayerDistance.Far)
                    {
                        playerState = PlayerState.FollowPlayer;
                        attackState = AttackState.None;
                    }
                    
                    break;

                case Formation.LockTarget:
                    if (playerDistance == PlayerDistance.Free || playerDistance == PlayerDistance.Close)
                    {
                        playerState = PlayerState.Combat;
                        attackState = AttackState.BasicAttack;
                    }
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
                case (PlayerState.FollowPlayer, _):
                    FollowPlayer();
                    break;

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

        private void FollowPlayer()
        {
            mover.MoveTo(mainPlayer.transform.position);
        }
    }
}
