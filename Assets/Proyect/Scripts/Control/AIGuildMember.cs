using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Control
{
    public class AIGuildMember : Character, IPlayable, ISubordinate
    {
        [SerializeField] Formation formation;
        [SerializeField] PlayerState playerState;
        [SerializeField] Character mainPlayer;
        [SerializeField] PlayerDistance playerDistance;
        [SerializeField] AttackState attackState;
        [SerializeField] float fellowGap;

        #region Enums

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
        #endregion

        const float closeDistance = 3;
        const float freeDistance = 6;
        const float farDistance = 9;

        public event Action OnCombatStarted;

        public bool IsControlled { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public Character[] Fellows { get; set; }
        public float FellowGap { get => fellowGap; }
        public Formation Formation { get => formation; }

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

        public void SetFormation(Vector2 formation)
        {
            //if (playerState != PlayerState.Combat) return;
                
            this.formation = formation switch
            {
                { y: 1 } => Formation.Follow,
                { y: -1 } => Formation.LockTarget,
                { x: -1 } => Formation.Protect,
                { x: 1 } => Formation.Free,
                _ => this.formation,
            };
            print ("Current formation: \t" + this.formation.ToString());
        }

        public void SetMainPlayer(Character character)
        {
            mainPlayer = character;
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

            ActionManager();
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
            mover.FollowWithDistance(mainPlayer.mover, fellowGap, Fellows);
        }
    }
}
