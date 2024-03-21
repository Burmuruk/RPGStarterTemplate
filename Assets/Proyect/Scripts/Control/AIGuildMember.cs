using Burmuruk.Tesis.Stats;
using Burmuruk.Utilities;
using System;
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

        object formationArgs;
        Transform m_target;

        #region Enums

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
        #endregion

        const float closeDistance = 3;
        const float freeDistance = 6;
        const float farDistance = 9;

        CoolDownAction cdTeleport;

        public bool IsControlled { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public Character[] Fellows { get; set; }
        public float FellowGap { get => fellowGap; }
        public Formation Formation { get => formation; }
        public PlayerState State { get => playerState; set => playerState = value; }

        protected override void Start()
        {
            base.Start();

            cdTeleport = new CoolDownAction(1.5f);
        }

        protected override void FixedUpdate()
        {
            playerDistance = Vector3.Distance(mainPlayer.transform.position, transform.position) switch
            {
                <= closeDistance => PlayerDistance.Close,
                <= freeDistance => PlayerDistance.Free,
                <= farDistance => PlayerDistance.Far,
                > farDistance => PlayerDistance.FarAway
            };

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

        public void SetFormation(Vector2 formation, object args)
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

            formationArgs = args;
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

                ActionManager();
                MovementManager();
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
                    else if (playerDistance == PlayerDistance.Far)
                    {
                        playerState = PlayerState.FollowPlayer;
                        attackState = AttackState.None;
                    }

                    break;

                case Formation.Protect:

                    if (playerDistance == PlayerDistance.Close)
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
                    else
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
            MovementManager();
        }

        protected override void ActionManager()
        {
            base.ActionManager();

            switch (playerState, attackState)
            {
                case (PlayerState.FollowPlayer, _):
                    break;

                case (PlayerState.Combat, AttackState.BasicAttack):
                    if (isTargetFar || isTargetClose)
                    {
                        if (formation == Formation.LockTarget)
                        {
                            m_target = ((Character)formationArgs).transform;
                        }
                        else
                        {
                            m_target = GetNearestTarget(eyesPerceibed);
                        }

                        fighter.SetTarget(m_target);
                        fighter.BasicAttack();
                    }
                    break;

                case (PlayerState.Teleporting, _):
                    Invoke("MoveCloseToPlayer", 1);
                    break;
            }
        }

        protected override void MovementManager()
        {
            base.MovementManager();

            switch (playerState, attackState)
            {
                case (PlayerState.FollowPlayer, _):
                    FollowPlayer();
                    break;

                case (PlayerState.Combat, AttackState.BasicAttack):
                    if (isTargetFar || isTargetClose)
                    {
                        if (formation == Formation.Protect) break;

                        var dis = Inventary.EquipedWeapon.MinDistance * .8f;
                        if (Vector3.Distance(m_target.position, transform.position) > dis)
                        {
                            var destiniy = (transform.position - m_target.position).normalized * dis;
                            destiniy += m_target.position;
                            mover.MoveTo(destiniy);
                            Debug.DrawRay(destiniy, Vector3.up * 5);
                        }
                    }
                    break;

                case (PlayerState.Teleporting, _):
                    Invoke("MoveCloseToPlayer", 1);
                    break;
            }
        }

        private void MoveCloseToPlayer()
        {
            var (x, z) = (Mathf.Cos(UnityEngine.Random.Range(-1, 1)), Mathf.Sin(UnityEngine.Random.Range(-1, .1f)));
            var dis = freeDistance / 2;

            var pos = new Vector3(x * dis, mainPlayer.transform.position.y, z * dis);
            pos = pos.normalized * UnityEngine.Random.Range(closeDistance, freeDistance);

            if (cdTeleport.CanUse)
            {
                StartCoroutine(cdTeleport.CoolDown());
                mover.ChangePositionTo(transform, mainPlayer.transform.position + pos);
            }
        }

        private Transform GetNearestTarget(Collider[] eyesPerceibed)
        {
            (Transform enemy, float dis) closest = (null, float.MaxValue);

            foreach (var enemy in eyesPerceibed)
            {
                if (Vector3.Distance(enemy.transform.position, transform.position) is var d && d < closest.dis)
                {
                    closest = (enemy.transform, d);
                }
            }

            return closest.enemy;
        }

        private void FollowPlayer()
        {
            mover.FollowWithDistance(mainPlayer.mover, fellowGap, Fellows);
        }
    }
}
