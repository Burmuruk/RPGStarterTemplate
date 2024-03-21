using Burmuruk.AI.PathFinding;
using Burmuruk.Tesis.Control;
using Burmuruk.Tesis.Stats;
using Burmuruk.WorldG.Patrol;
using System;
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

        public float wanderDisplacement;
        public float wanderRadious;
        public bool usePathFinding = false;
        bool m_canMove = false;
        Vector3 target = Vector3.zero;
        bool isMoving = false;
        PathFinder m_pathFinder;
        INodeListSupplier nodeList;

        public event Action OnFinished = delegate { };

        public Vector3 CurDirection { get; private set; }
        public Vector3 Veloctiy { get => m_rb.velocity; }

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

        public void SetConnections(INodeListSupplier nodeList)
        {
            m_pathFinder = new PathFinder(nodeList);
            this.nodeList = nodeList;

            m_pathFinder.OnPathCalculated += () =>
            {
                print("Finished");
                m_canMove = true;

                if (m_pathFinder.BestRoute == null || m_pathFinder.BestRoute.Count == 0)
                {
                    FinishAction();
                    isMoving = false;
                    print("Path not founded");
                    return;
                }

                target = m_pathFinder.BestRoute.Last.Value.Position;
                print(m_pathFinder.BestRoute.Last.Value.ID);
                UnityEngine.Debug.DrawLine(transform.position, target, Color.black);

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
            if (isMoving) return;
            //UnityEngine.Debug.DrawRay(point, Vector3.up * 8);
            isMoving = true;

            try
            {
                var nearest = nodeList.FindNearestNode(point);

                if (nearest == null)
                {
                    isMoving = false;
                    return;
                }

                if (nodeList.ValidatePosition(point, nearest))
                {
                    target = point;
                    //m_pathFinder.Find_BestRoute<AStar>((transform.position, target)); 
                }
                else
                {
                    isMoving =false;
                    return;
                    target = nearest.Position;
                    m_pathFinder.Find_BestRoute<AStar>((transform.position, nearest.Position));
                }


                m_canMove = true;
            }
            catch (Exception e)
            {
                m_canMove = true;
                isMoving = false;
                throw e;
            }
        }

        public void FollowWithDistance(Movement target, float gap, params Character[] fellows)
        {
            if (isMoving) return;

            Vector3 point = SteeringBehaviours.GetFollowPosition(target, this, gap, fellows);
            Debug.DrawRay(point, Vector3.up * 8, Color.red);
            MoveTo(point);
            //isMoving = true;

            //try
            //{
            //    m_canMove = true;
            //    m_pathFinder.Find_BestRoute<AStar>((transform.position, point));
            //}
            //catch (Exception e)
            //{
            //    m_canMove = true;
            //    isMoving = false;
            //    throw e;
            //}
        }

        public void ChangePositionTo(Transform agent, Vector3 point)
        {
            var nearest = nodeList.FindNearestNode(point);

            if (nearest == null) return;

            agent.position = nearest.Position;
        }

        public void Flee()
        {

        }

        public void Pursue()
        {

        }

        private void Move()
        {
            if (!m_canMove) return;

            //UnityEngine.Debug.DrawRay(target, Vector3.up * 18, Color.red);

            m_rb.velocity = SteeringBehaviours.Seek2D(this, target);
            var pos1 = new Vector3(transform.position.x, 0, transform.position.z);
            var pos2 = new Vector3(target.x, 0, target.z);

            if (Vector3.Distance(pos1, pos2) is var d && d> SlowingRadious)
            {
                CurDirection = m_rb.velocity.normalized;
            }
            else if (d <= m_threshold)
            {
                StopAction();
            }
            else
            {
                m_rb.velocity = SteeringBehaviours.Arrival(this, target, SlowingRadious, m_threshold);
                CurDirection = Vector3.zero;
            }

            SteeringBehaviours.LookAt(transform, new(m_rb.velocity.x, 0, m_rb.velocity.z));
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
            //target = default;
            m_rb.velocity = new(0, m_rb.velocity.y, 0);
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
            isMoving = false;
            m_canMove = false;
            //print("PatrolFinished");
        }
    }
}
