using UnityEngine;
using Burmuruk.Tesis.Stats;
using UnityEngine.SocialPlatforms;
using System;

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

        StatsManager m_stats;
        protected Collider[] eyesPerceibed, earsPerceibed;
        protected bool isFar = false;
        protected bool isClose = false;

        protected virtual void Awake()
        {
            m_mover = GetComponent<Movement.Movement>();
            m_fighter = GetComponent<Fighting.Fighter>();
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
            eyesPerceibed = Physics.OverlapSphere(farPercept.position, m_stats.EyesRadious);
            earsPerceibed = Physics.OverlapSphere(closePercept.position, m_stats. EarsRadious);
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

            for (int i = 0; i < perceibed.Length; i++)
            {
                ref var cur = ref perceibed[i];

                if (cur.GetComponent<EnemyController>() is var enemy && enemy != null)
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual void DecisionManager()
        {

        }

        protected virtual void ActionManager()
        {

        }
    }
}
