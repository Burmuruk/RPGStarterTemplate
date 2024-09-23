using Burmuruk.Tesis.Combat;
using Burmuruk.Tesis.Interaction;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using Burmuruk.Utilities;
using Burmuruk.WorldG.Patrol;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Control.AI
{
    public class AIEnemyController : Character
    {
        [SerializeField] List<InventoryItem> itemsToDrop;
        protected AIEnemyController leader;
        protected List<(float value, Character enemy)> rage;
        protected List<AbiltyTrigger> abilities = new();
        [SerializeField] PatrolController patrolController;
        List<Character> _enemies = new();
        protected Character m_target;

        [SerializeField] protected PlayerAction playerAction;
        [SerializeField] protected AttackState attackState;
        [SerializeField] protected PlayerDistance playerDistance;
        [SerializeField] protected PlayerState playerState;
        [SerializeField] protected RageState rageState;
        [SerializeField] protected Awareness awareness;
        [SerializeField] protected LeaderOrder curOrder;

        public Character CurEnemy { get; private set; }

        #region Enums
        public enum PlayerAction
        {
            None,
            Combat,
            Patrol,
            Dead,
            Flee,
            Following
        }

        public enum AttackState
        {
            None,
            BasicAttack,
            SpecialAttack,
            Cover
        }

        public enum PlayerDistance
        {
            None,
            Close,
            Free,
            Far,
            FarAway
        }

        public enum PlayerState
        {
            None,
            Normal,
            Danger,
        }

        public enum RageState
        {
            None,
            Low,
            Medium,
            High,
            UltraHigh,
            Revenge
        }

        public enum Awareness
        {
            None,
            Front,
            Surounded,
            Alone,
            Threatened
        } 

        public enum LeaderOrder
        {
            None,
            Attack,
            Follow
        }
        #endregion

        public class AbiltyTrigger
        {
            public Ability ability;
            public CoolDownAction cd;
            public int uses;

            public AbiltyTrigger(Ability ability, CoolDownAction coolDown)
            {
                this.ability = ability;
                this.cd = coolDown;
                uses = 0;
            }

            public void UseAbility()
            {
                uses++;
                //ability.Use();
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public void Restart()
        {

        }

        public void SetLeader(AIEnemyController leader)
        {
            if (!leader)
            {
                leader = null;
                curOrder = LeaderOrder.None;
                return;
            }

            this.leader = leader;
        }

        public void SetTarget(Character target)
        {
            m_target = target;
        }

        public void SetOrder(LeaderOrder order)
        {
            curOrder = order;
        }

        public override void SetStats(BasicStats stats)
        {
            base.SetStats(stats);

            SetAbilities();
            //statsList.OnDied += DropItem;
            //patrolController = new PatrolController();

            if (patrolController != null)
            {
                patrolController.Initialize(mover, mover.Finder);
            }
        }

        protected override void PerceptionManager()
        {
            base.PerceptionManager();
        }

        protected override void DecisionManager()
        {
            if (playerAction == PlayerAction.Dead) return;

            try
            {

                CheckLeader();
                CheckOwnState();
                FindEnemies();
                ChooseAttack();
                TryTakeCover();
                CheckPatrolPath();
            }
            catch (NotImplementedException) { }
            
            ActionManager();
            MovementManager();
        }

        protected override void ActionManager()
        {
            base.ActionManager();
        }

        protected virtual void CheckPatrolPath()
        {
            if (!patrolController) return;
            if (mover == null || mover.nodeList == null) return;

            playerAction = PlayerAction.Patrol;
            patrolController.StartPatrolling();
        }

        protected virtual void CheckLeader()
        {
            if (!leader) return;

            switch (curOrder)
            {
                case LeaderOrder.Attack:
                    playerAction = PlayerAction.Combat;
                    break;

                case LeaderOrder.Follow:
                    playerAction = PlayerAction.Following;
                    break;

                default:
                    return;
            }

            throw new NotImplementedException();
        }

        protected virtual void CheckOwnState()
        {
            if (health.HP < health.MaxHp * .4f)
            {
                TryHeal();
                throw new NotImplementedException();
            }
            //else if (Inventory.EquipedWeapon.Ammo <= 0)
            //{
            //    //Reload
            //    throw new NotImplementedException();
            //}

            return;
        }

        protected virtual void FindEnemies()
        {
            if (IsTargetClose || IsTargetFar)
            {
                playerAction = PlayerAction.Combat;
                throw new NotImplementedException();
            }

            return;
        }

        protected virtual void ChooseAttack()
        {
            
        }

        protected virtual void TryTakeCover()
        {
            return;
        }

        protected virtual void TryHeal()
        {
            return;
        }

        protected virtual void ChooseAbility()
        {
            foreach (var ability in abilities)
            {
                if (ability.cd.CanUse)
                {
                    ability.UseAbility();
                    StartCoroutine(ability.cd.CoolDown());
                }
            }
        }

        private void SetAbilities()
        {
            var items = (inventory as InventoryEquipDecorator).Equipped.GetItems((int)EquipmentType.Ability);

            if (items == null) return;
            
            foreach (var item in items)
            {
                var ability = (Ability)item;
                var cd = new CoolDownAction(ability.CoolDown);

                abilities.Add(new AbiltyTrigger(ability, cd));
            }
        }

        private void MoveCloseToPlayer(float minDis, float maxDis, Transform player)
        {
            var (x, z) = (Mathf.Cos(UnityEngine.Random.Range(-1, 1)), Mathf.Sin(UnityEngine.Random.Range(-1, .1f)));
            var dis = minDis;

            var pos = new Vector3(x * dis, player.transform.position.y, z * dis);
            pos = pos.normalized * UnityEngine.Random.Range(minDis, maxDis);
        }

        protected override void Dead()
        {
            base.Dead();

            DropItem();
        }

        public void DropItem()
        {
            if (itemsToDrop == null || itemsToDrop.Count <= 0) return;

            var rand = UnityEngine.Random.Range(0, itemsToDrop.Count);

            FindObjectOfType<PickupSpawner>().AddItem(itemsToDrop[rand], transform.position);

            //var item = Instantiate(itemsToDrop.Items[rand].Prefab);
            //item.transform.position = transform.position;
        }
    }
}
