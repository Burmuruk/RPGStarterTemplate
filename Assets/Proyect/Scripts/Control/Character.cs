using Assets.Proyect.Scripts.Control;
using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using UnityEngine;

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

        [Space(), Header("Settings")]
        [SerializeField] protected string enemyTag;
        //[SerializeField] protected float farRadious;
        //[SerializeField] protected float closeRadious;

        [HideInInspector] public Movement.Movement mover;
        [HideInInspector] public Fighting.Fighter fighter;
        [HideInInspector] public StatsManager stats;
        [HideInInspector] Inventary inventary;

        protected Collider[] eyesPerceibed, earsPerceibed;
        protected bool isTargetFar = false;
        protected bool isTargetClose = false;

        public Inventary Inventary { get => inventary; }

        protected virtual void Awake()
        {
            mover = GetComponent<Movement.Movement>();
            fighter = GetComponent<Fighting.Fighter>();
            stats = GetComponent<StatsManager>();
            inventary = gameObject.GetComponent<Inventary>();
        }

        protected virtual void Start()
        {

        }

        protected virtual void Update()
        {
            PerceptionManager();
            DecisionManager();
        }

        protected virtual void FixedUpdate()
        {
            eyesPerceibed = Physics.OverlapSphere(farPercept.position, stats.EyesRadious, 1<<10);
            earsPerceibed = Physics.OverlapSphere(closePercept.position, stats.EarsRadious, 1 << 10);
        }

        private void OnDrawGizmosSelected()
        {
            if (!stats || !farPercept || !closePercept) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(farPercept.position, stats.EyesRadious);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(closePercept.position, stats.EarsRadious);
        }

        private void PerceptionManager()
        {
            if (hasFarPerception)
            {
                isTargetFar = PerceptEnemy(eyesPerceibed);
            }
            if (hasClosePerception)
            {
                isTargetClose = PerceptEnemy(earsPerceibed);
            }
        }

        protected virtual bool PerceptEnemy(Collider[] perceibed)
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

        protected virtual void DecisionManager()
        {

        }

        protected virtual void ActionManager()
        {

        }

        protected virtual void MovementManager()
        {

        }
    }
}
