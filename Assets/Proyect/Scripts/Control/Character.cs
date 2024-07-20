using System;
using System.Collections.Generic;
using UnityEngine;
using Burmuruk.Tesis.Stats;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Combat;

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
        [SerializeField] protected Equipment equipment;

        [Space(), Header("Settings")]
        [SerializeField] protected string enemyTag;
        //[SerializeField] protected float farRadious;
        //[SerializeField] protected float closeRadious;

        [HideInInspector] public Movement.Movement mover;
        [HideInInspector] public Fighter fighter;
        [HideInInspector] public BasicStats stats;
        [HideInInspector] protected IInventory inventory;

        protected Collider[] eyesPerceibed, earsPerceibed;
        protected bool isTargetFar = false;
        protected bool isTargetClose = false;

        public virtual event Action<bool> OnCombatStarted;

        public IInventory Inventory { get => inventory; set => inventory = value; }
        public Collider[] CloseEnemies { get => earsPerceibed; }
        public Collider[] FarEnemies { get => eyesPerceibed; }
        public bool IsTargetFar { get => isTargetFar; }
        public bool IsTargetClose { get => isTargetClose; }
        public Equipment Equipment { get => equipment; }

        protected virtual void Awake()
        {
            mover = GetComponent<Movement.Movement>();
            fighter = GetComponent<Fighter>();
            stats = GetComponent<Stats.BasicStats>();
            inventory = gameObject.GetComponent<IInventory>();
        }

        protected virtual void Update()
        {
            DecisionManager();
        }

        protected virtual void FixedUpdate()
        {
            eyesPerceibed = Physics.OverlapSphere(farPercept.position, stats.eyesRadious, 1<<10);
            earsPerceibed = Physics.OverlapSphere(closePercept.position, stats.earsRadious, 1 << 10);

            PerceptionManager();
        }

        private void OnDrawGizmosSelected()
        {
            if (!stats.Initilized || !farPercept || !closePercept) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(farPercept.position, stats.eyesRadious);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(closePercept.position, stats.earsRadious);
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
    }
}
