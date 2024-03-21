using Burmuruk.AI.PathFinding;
using Burmuruk.Tesis.Control;
using Burmuruk.Tesis.Stats;
using Burmuruk.WorldG.Patrol;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Movement
{
    public enum MovementState
    {
        None,
        Moving,
        FollowingPath
    }

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
        PathFinder m_pathFinder;
        INodeListSupplier nodeList;

        Vector3 target = Vector3.zero;
        public float wanderDisplacement;
        public float wanderRadious;
        public bool usePathFinding = false;
        bool m_canMove = false;
        MovementState m_state = MovementState.None;
        IPathNode m_curNode;
        
        LinkedList<IPathNode> m_curPath;
        IEnumerator<IPathNode> enumerator;


        public event Action OnFinished = delegate { };

        public Vector3 CurDirection { get; private set; }
        public Vector3 Veloctiy { get => m_rb.velocity; }
        public bool IsMoving
        {
            get
            {
                if (m_state == MovementState.Moving || m_state == MovementState.FollowingPath)
                    return true;

                return false;
            }
        }

        float SlowingRadious
        {
            get
            {
                if (m_inventary == null)
                    return 3;

                return m_inventary.EquipedWeapon.MinDistance;
            }
        }

        #region Unity mehthods
        private void Awake()
        {
            m_rb = GetComponent<Rigidbody>();
            m_statsManager = GetComponent<StatsManager>();
            m_scheduler = new MovementSchuduler();

            //m_patrolController = gameObject.GetComponent<PatrolController>();
            //m_patrolController.OnFinished += m_patrolController.Execute_Tasks;
        }

        private void Start()
        {

        }

        private void FixedUpdate()
        {
            Move();
        } 
        #endregion

        public void SetConnections(INodeListSupplier nodeList)
        {
            m_pathFinder = new PathFinder(nodeList);
            this.nodeList = nodeList;

            m_pathFinder.OnPathCalculated += () =>
            {
                if (m_pathFinder.BestRoute == null || m_pathFinder.BestRoute.Count == 0)
                {
                    FinishAction();
                    return;
                }

                m_state = MovementState.FollowingPath;
                m_curPath = m_pathFinder.BestRoute;
                GetNextNode();

                IPathNode lastNode = null;
                foreach (var node in m_pathFinder.BestRoute)
                {
                    if (lastNode != null)
                        UnityEngine.Debug.DrawLine(lastNode.Position, node.Position, Color.black, 5);

                    lastNode = node;
                }
            };
        }

        public void MoveTo(Vector3 point)
       {
            if (IsMoving) return;
            m_state = MovementState.Moving;

            try
            {
                var nearest = nodeList.FindNearestNode(point);

                if (nearest == null)
                {
                    m_state = MovementState.Moving;
                    return;
                }

                if (nodeList.ValidatePosition(point, nearest))
                {
                    target = point;
                    //m_pathFinder.Find_BestRoute<AStar>((transform.position, target)); 
                }
                else
                {
                    m_state = MovementState.None;
                    return;
                    target = nearest.Position;
                    m_pathFinder.Find_BestRoute<AStar>((transform.position, nearest.Position));
                }
            }
            catch (Exception e)
            {
                m_state = MovementState.None;
                throw e;
            }
        }

        public void FollowWithDistance(Movement target, float gap, params Character[] fellows)
        {
            if (IsMoving) return;

            Vector3 point = SteeringBehaviours.GetFollowPosition(target, this, gap, fellows);
            m_state = MovementState.FollowingPath;

            try
            {
                m_pathFinder.Find_BestRoute<AStar>((transform.position, point));
            }
            catch (Exception e)
            {
                m_state = MovementState.None;
                throw e;
            }
        }

        public void ChangePositionTo(Transform agent, Vector3 point)
        {
            if (IsMoving) FinishAction();

            var nearest = nodeList.FindNearestNode(point);

            if (nearest == null) return;

            agent.position = nearest.Position;
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
            if (m_state == MovementState.FollowingPath)
            FinishAction();
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
            m_rb.velocity = new(0, m_rb.velocity.y, 0);
            m_state = MovementState.None;
            m_curPath = null;
            enumerator = null;
            m_curNode = null;
        }

        public void Flee()
        {

        }

        public void Pursue()
        {

        }

        private void Move()
        {
            if (!m_canMove && !IsMoving) return;

            m_rb.velocity = SteeringBehaviours.Seek2D(this, target);
            var pos1 = new Vector3(transform.position.x, 0, transform.position.z);
            var pos2 = new Vector3(target.x, 0, target.z);

            if (Vector3.Distance(pos1, pos2) is var d && d > SlowingRadious)
            {
                CurDirection = m_rb.velocity.normalized;
            }
            else if (d <= m_threshold && 
                (m_state == MovementState.Moving ||
                (m_state == MovementState.FollowingPath && !GetNextNode())))
            {
                FinishAction();
            }
            else if (m_state == MovementState.Moving ||
                (m_state == MovementState.FollowingPath && m_curNode.ID != m_curPath.Last.Value.ID))
            {
                m_rb.velocity = SteeringBehaviours.Arrival(this, target, SlowingRadious, m_threshold);
                CurDirection = Vector3.zero;
            }

            SteeringBehaviours.LookAt(transform, new(m_rb.velocity.x, 0, m_rb.velocity.z));
        }

        private bool GetNextNode()
        {
            if (m_curPath != null)
            {
                if (enumerator == null)
                {
                    enumerator = m_curPath.GetEnumerator();
                }

                if (enumerator.MoveNext())
                {
                    target = enumerator.Current.Position;
                    m_curNode = enumerator.Current;
                    return true;
                }
            }

            return false;
        }
    }
}
