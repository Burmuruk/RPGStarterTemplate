using System;
using System.Collections.Generic;
using UnityEngine;
using Burmuruk.Tesis.Stats;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Combat;
using Burmuruk.Tesis.Utilities;

namespace Burmuruk.Tesis.Control
{
    public class Character : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] protected Transform farPercept;
        [SerializeField] protected Transform closePercept;
        [Space(), Header("Status"), Space()]
        [SerializeField] protected bool hasFarPerception;
        [SerializeField] protected bool hasClosePerception;
        [SerializeField] CharacterType characterType;
        [SerializeField] protected Equipment equipment;

        [Space(), Header("Settings")]
        [SerializeField] protected string enemyTag;
        //[SerializeField] protected float farRadious;
        //[SerializeField] protected float closeRadious;

        [HideInInspector] protected Health health;
        [HideInInspector] public Movement.Movement mover;
        [HideInInspector] public Fighter fighter;
        [HideInInspector] public BasicStats stats = new();
        [HideInInspector] public ActionScheduler actionScheduler = new();
        [HideInInspector] protected IInventory inventory;

        protected Collider[] eyesPerceibed, earsPerceibed;
        protected bool isTargetFar = false;
        protected bool isTargetClose = false;

        public virtual event Action<bool> OnCombatStarted;

        public Health Health { get => health; }
        public IInventory Inventory {
            get
            {
                return (inventory ??= gameObject.GetComponent<IInventory>());
            }
            set => inventory = value;
        }
        public Collider[] CloseEnemies { get => earsPerceibed; }
        public Collider[] FarEnemies { get => eyesPerceibed; }
        public bool IsTargetFar { get => isTargetFar; }
        public bool IsTargetClose { get => isTargetClose; }
        public ref Equipment Equipment { get => ref equipment; }
        public CharacterType CharacterType { get => characterType; }

        protected virtual void Awake()
        {
            GetComponents();
            health.OnDied += Dead;
        }

        protected virtual void Update()
        {
            if (health.HP <= 0) return;

            DecisionManager();
        }

        protected virtual void FixedUpdate()
        {
            if (health.HP <= 0) return;

            eyesPerceibed = Physics.OverlapSphere(farPercept.position, stats.eyesRadious, 1 << 10);
            earsPerceibed = Physics.OverlapSphere(closePercept.position, stats.earsRadious, 1 << 10);

            PerceptionManager();
        }

        private void OnDrawGizmosSelected()
        {
            if (/*!stats.Initilized ||*/ !farPercept || !closePercept) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(farPercept.position, stats.eyesRadious);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(closePercept.position, stats.earsRadious);
        }

        public void SetUpMods()
        {
            //ModsList.AddVariable(this, ModifiableStat.HP, _=>health.HP, (value) => health.HP = value);
            ModsList.AddVariable((Character)this, ModifiableStat.Speed, () => stats.Speed, (value) => stats.Speed = value);
            ModsList.AddVariable((Character)this, ModifiableStat.BaseDamage, () => stats.Damage, (value) => { stats.Damage = (int)value; });
            ModsList.AddVariable((Character)this, ModifiableStat.GunFireRate, () => stats.DamageRate, (value) => { stats.damageRate = value; });
            ModsList.AddVariable((Character)this, ModifiableStat.MinDistance, () => stats.MinDistance, (value) => { stats.MinDistance = value; });
        }

        public virtual void SetStats(BasicStats stats)
        {
            this.stats = stats;
            var invent = Inventory as InventoryEquipDecorator;
            if (!fighter)
                GetComponents();
            fighter.Initilize(invent, ref this.stats);
            mover.Initialize(invent, actionScheduler, this.stats);
        }

        protected virtual void PerceptionManager()
        {
            if (hasFarPerception)
            {
                isTargetFar = PerceptEnemy(ref eyesPerceibed);
            }
            if (hasClosePerception)
            {
                isTargetClose = PerceptEnemy(ref earsPerceibed);
            }
        }

        private void GetComponents()
        {
            health ??= GetComponent<Health>();
            mover ??= GetComponent<Movement.Movement>();
            fighter ??= GetComponent<Fighter>();
        }

        protected virtual bool PerceptEnemy(ref Collider[] perceibed)
        {
            if (perceibed == null) return false;

            List<Collider> enemies = new();
            bool founded = false;

            for (int i = 0; i < perceibed.Length; i++)
            {
                ref var cur = ref perceibed[i];

                if (cur.CompareTag(enemyTag))
                {
                    enemies.Add(cur);
                    founded = true;
                }
            }

            if (enemies.Count > 0)
            {
                perceibed = enemies.ToArray();
            }

            return founded;
        }

        protected virtual void DecisionManager() { }

        protected virtual void ActionManager() { }

        protected virtual void MovementManager() { }

        protected Transform GetNearestTarget(Collider[] eyesPerceibed)
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

        protected virtual void Dead()
        {
            gameObject.SetActive(false);
            StopAllCoroutines();
            FindObjectOfType<BuffsManager>().RemoveAllBuffs(this);
        }
    }
}
