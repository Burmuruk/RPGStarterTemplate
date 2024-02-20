using Burmuruk.Tesis.Stats;
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

        protected Movement.Movement m_mover;
        protected Fighting.Fighter m_fighter;
        protected StatsManager m_stats;
        protected Inventary m_inventary;

        protected Collider[] eyesPerceibed, earsPerceibed;
        protected bool isFar = false;
        protected bool isClose = false;

        protected virtual void Awake()
        {
            m_mover = GetComponent<Movement.Movement>();
            m_fighter = GetComponent<Fighting.Fighter>();
            m_stats = GetComponent<StatsManager>();
            m_inventary = gameObject.GetComponent<Inventary>();
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
            eyesPerceibed = Physics.OverlapSphere(farPercept.position, m_stats.EyesRadious, 1<<10);
            earsPerceibed = Physics.OverlapSphere(closePercept.position, m_stats. EarsRadious, 1 << 10);
        }

        private void PerceptionManager()
        {
            if (hasFarPerception)
            {
                isFar = PerceptEnemy(eyesPerceibed);
            }
            if (hasClosePerception)
            {
                isClose = PerceptEnemy(earsPerceibed);
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

                if (cur.GetComponent<EnemyController>() is var enemy && enemy != null)
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
    }
}
