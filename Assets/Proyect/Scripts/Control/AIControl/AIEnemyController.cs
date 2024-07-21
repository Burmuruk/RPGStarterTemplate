using Burmuruk.Tesis.Combat;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Utilities;
using Burmuruk.WorldG.Patrol;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Control.AI
{
    public class AIEnemyController : Character
    {
        //[SerializeField] ItemsList itemsList;
        protected AIEnemyController leader;
        protected List<(float value, Character enemy)> rage;
        protected List<AbiltyTrigger> abilities = new();
        PatrolController patrolController;

        protected PlayerAction playerAction;
        protected AttackState attackState;
        protected PlayerDistance playerDistance;
        protected PlayerState playerState;
        protected RageState rageState;
        protected Awareness awareness;
        protected LeaderOrder curOrder;

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

        protected virtual void Start()
        {
            SetAbilities();
            //stats.OnDied += DropItem;
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

        public void SetOrder(LeaderOrder order)
        {
            curOrder = order;
        }

        protected override void PerceptionManager()
        {
            base.PerceptionManager();
        }

        protected override void DecisionManager()
        {
            base.DecisionManager();
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
            finally
            {
                ActionManager();
                MovementManager();
            }
        }

        protected override void ActionManager()
        {
            base.ActionManager();
        }

        protected virtual void CheckPatrolPath()
        {
            var collides = Physics.OverlapSphere(transform.position, 2);

            foreach (var collider in collides)
            {
                if (collider.GetComponent<Spline>() is var spline && spline)
                {
                    patrolController ??= GetComponent<PatrolController>();

                    
                }
            }
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
            
            for (int i = 0; i < items.Count; i++)
            {
                var ability = (Ability)items[i];
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

        public void DropItem()
        {
            //if (itemsList.Items == null || itemsList.Items.Count <= 0) return;

            //var rand = UnityEngine.Random.Range(0, itemsList.Items.Count);

            //var item = Instantiate(itemsList.Items[rand].Prefab);
            //item.transform.position = transform.position;
        }
    }
}
