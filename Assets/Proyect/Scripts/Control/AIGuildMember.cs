using Burmuruk.Tesis.Control.AI;
using Burmuruk.Tesis.Stats;
using Burmuruk.Utilities;
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

        object formationArgs;
        Transform m_target;
        CoolDownAction cdTeleport;
        public int id = -1;

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

        public event Action OnEnemyDetected;
        public  event Action OnFormationChanged;
        public override event Action<bool> OnCombatStarted;

        public bool IsControlled { get; set; }
        public Character[] Fellows { get; set; }
        public float FellowGap { get => fellowGap; }
        public Formation Formation { get => formation; }
        public PlayerState PlayerState
        {
            get => playerState;
            set
            {
                if (playerState == PlayerState.Combat && value != PlayerState.Combat)
                {
                    playerState = value;
                    OnCombatStarted?.Invoke(false);
                    return;
                }

                playerState = value;
            }
        }

        protected virtual void Start()
        {
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

        private void LateUpdate()
        {
            if (playerState == PlayerState.Combat && !isTargetFar && !isTargetClose)
            {
                Collider closest = null;
                List<(Component enemy, float distance)> closestEnemies = new();

                foreach (Character character in Fellows)
                {
                    if (character.IsTargetClose || character.IsTargetFar)
                    {
                        closestEnemies.Add(GetClosestEnemy(character.CloseEnemies));
                    }
                }

                if (closestEnemies.Count > 0)
                {

                }
                else
                {
                    OnCombatStarted(false);
                }
            }
        }

        public void DisableControll()
        {
            IsControlled = false;
        }

        public void EnableControll()
        {
            IsControlled = true;
        }

        public void SetFormation(Formation formation, object args)
        {
            //if (playerState != PlayerAction.Combat) return;

            this.formation = formation;

            formationArgs = args;
            print ("Current formation: \t" + this.formation.ToString());
        }

        public void SetMainPlayer(Character character)
        {
            mainPlayer = character;
        }

        public void SetTarget(AIEnemyController enemy)
        {
            m_target = enemy.transform;
        }

        protected override void DecisionManager()
        {
            if (IsControlled)
            {
                ControlledDecisionManager();
                ActionManager();
                //MovementManager();
                return;
            }

            if (playerDistance == PlayerDistance.FarAway)
            {
                PlayerState = PlayerState.Teleporting;

                ActionManager();
                MovementManager();
                return;
            }

            switch (formation)
            {
                case Formation.Follow:

                    PlayerState = PlayerState.FollowPlayer;
                    attackState = AttackState.None;
                    break;

                case Formation.None:
                case Formation.Free:

                    if (playerDistance == PlayerDistance.Free || playerDistance == PlayerDistance.Close)
                    {
                        if ((isTargetFar || isTargetClose) && 
                            Vector3.Distance(transform.position, 
                                GetNearestTarget(eyesPerceibed).position) < freeDistance)
                        {
                            PlayerState = PlayerState.Combat;
                            attackState = AttackState.BasicAttack;
                        }
                        else
                        {
                            PlayerState = PlayerState.None;
                            attackState = AttackState.None;
                        }
                    }
                    else if (playerDistance == PlayerDistance.Far)
                    {
                        PlayerState = PlayerState.FollowPlayer;
                        attackState = AttackState.None;
                    }

                    break;

                case Formation.Protect:

                    if (playerDistance == PlayerDistance.Close)
                    {
                        if ((isTargetFar || isTargetClose) &&
                            Vector3.Distance(transform.position, 
                            GetNearestTarget(eyesPerceibed).position) < stats.MinDistance)
                        {
                            PlayerState = PlayerState.Combat;
                            attackState = AttackState.BasicAttack;
                        }
                        else
                        {
                            PlayerState = PlayerState.None;
                            attackState = AttackState.None;
                        }
                    }
                    else
                    {
                        PlayerState = PlayerState.FollowPlayer;
                        attackState = AttackState.None;
                    }
                    
                    break;

                case Formation.LockTarget:
                    if (playerDistance == PlayerDistance.Free || playerDistance == PlayerDistance.Close)
                    {
                        PlayerState = PlayerState.Combat;
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
                    if (isTargetFar || isTargetClose || m_target)
                    {
                        if (formation == Formation.LockTarget)
                        {
                            m_target = ((Character)formationArgs).transform;
                        }
                        else if (m_target == null)
                        {
                            m_target = GetNearestTarget(eyesPerceibed);
                        }

                        OnCombatStarted?.Invoke(true);
                        fighter.SetTarget(m_target);
                        fighter.BasicAttack();
                    }
                    else if (m_target == null)
                    {
                        m_target = GetNearestTarget(eyesPerceibed);
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

                        var dis = stats.MinDistance * .8f;
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

        protected virtual void IdentifyHazards()
        {
            throw new NotImplementedException();
        }

        private void ControlledDecisionManager()
        {
            if (m_target || isTargetClose || isTargetFar)
            {
                OnCombatStarted?.Invoke(true);

                playerState = PlayerState.Combat;
                attackState = AttackState.BasicAttack;
            }
            else
                OnCombatStarted?.Invoke(false);
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

        private void FollowPlayer()
        {
            mover.FollowWithDistance(mainPlayer.mover, fellowGap, Fellows);
        }

        private void GetRemainEnemies()
        {
            
        }

        private (Component obj, float dis) GetClosestEnemy(Component[] enemies)
        {
            (int idx, float distance) closest = (0, float.MaxValue);

            for (int i = 0; i < enemies.Length; i++)
            {
                var dis = Vector3.Distance(enemies[i].transform.position, transform.position);
                if (dis < closest.distance)
                {
                    closest = (i, dis);
                }
            }

            return (enemies[closest.idx], closest.distance);
        }
    }
}
