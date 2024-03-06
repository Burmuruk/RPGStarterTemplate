using Burmuruk.AI.PathFinding;
using Burmuruk.Tesis.Stats;
using Burmuruk.WorldG.Patrol;
using System;
using System.Diagnostics;
using UnityEngine;

namespace Burmuruk.Tesis.Movement
{
    [RequireComponent(typeof(Rigidbody))]
    public class Movement : MonoBehaviour, IMoveAction
    {
        [SerializeField] float m_maxVel; 
        [SerializeField] float m_maxSteerForce;
        [SerializeField] float m_threshold;

        Rigidbody m_rb;
        StatsManager m_statsManager;
        Inventary m_inventary;
        MovementSchuduler m_scheduler;
        PatrolController m_patrolController;

        public float wanderDisplacement;
        public float wanderRadious;
        public bool usePathFinding = false;
        bool m_canMove = false;
        Vector3 target = Vector3.zero;
        bool isMoving = false;

        public event Action OnActionFinished = delegate { };

        public Vector3 CurDirection { get; private set; }

        float SlowingRadious
        {
            get
            {
                if (m_inventary == null)
                    return 3;

                return m_inventary.EquipedWeapon.MinDistance;
            }
        }

        private void Awake()
        {
            m_rb = GetComponent<Rigidbody>();
            m_statsManager = GetComponent<StatsManager>();
            m_scheduler = new MovementSchuduler();

            m_patrolController = gameObject.GetComponent<PatrolController>();
            m_patrolController.OnFinished += m_patrolController.Execute_Tasks;
        }

        private void FixedUpdate()
        {
            if (m_canMove)
            {
                Move();
            }
        }

        public void MoveTo(Vector3 point)
        {
            if (isMoving) return;

            target = point;
            m_canMove = true;
            m_patrolController.OnPatrolFinished += FinishAction;
            m_patrolController.CreatePatrolWithSpline<AStar>(transform.position, target, CyclicType.None);
            isMoving = true;
        }

        public void Flee()
        {

        }

        public void Pursue()
        {

        }

        private void Move()
        {
            if (Vector3.Distance(transform.position, target) > SlowingRadious)
            {
                m_rb.velocity = SteeringBehaviours.Seek2D(this, target);
                CurDirection = m_rb.velocity.normalized;
            }
            else
            {
                m_rb.velocity = SteeringBehaviours.Arrival(this, target, SlowingRadious, m_threshold);
                CurDirection = Vector3.zero;
            }

            SteeringBehaviours.LookAt(transform, m_rb.velocity);
        }

        /// <summary>
        /// Gives access to max speed of agent
        /// </summary>
        /// <returns>float m_speed</returns>
        public float GetSpeed()
        {
            return m_statsManager.Speed;
        }

        /// <summary>
        /// Gives access to max velocity of agent
        /// </summary>
        /// <returns>float m_MaxVel</returns>
        public float getMaxVel()
        {
            return m_maxVel;
        }

        /// <summary>
        /// Gives access to steer force of agent
        /// </summary>
        /// <returns>float m_MaxSteerForce</returns>
        public float getMaxSteerForce()
        {
            return m_maxSteerForce;
        }

        public void StopAction()
        {
            throw new System.NotImplementedException();
        }

        public void StartAction()
        {
            throw new System.NotImplementedException();
        }

        public void PauseAction()
        {
            throw new System.NotImplementedException();
        }

        public void FinishAction()
        {
            isMoving = false;
            m_patrolController.OnPatrolFinished -= FinishAction;
            print("PatrolFinished");
        }
    }
}
