using Burmuruk.AI.PathFinding;
using Burmuruk.Tesis.Control;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using Burmuruk.Tesis.Utilities;
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
        FollowingPath,
        Calculating
    }

    [RequireComponent(typeof(Rigidbody))]
    public class Movement : MonoBehaviour, IScheduledAction
    {
        [SerializeField] float m_maxVel; 
        [SerializeField] float m_maxSteerForce;
        [SerializeField] float m_threshold;
        [SerializeField] float m_slowingRadious;

        Rigidbody m_rb;
        BasicStats stats;
        InventoryEquipDecorator m_inventory;
        ActionScheduler m_scheduler;
        PathFinder m_pathFinder;
        public INodeListSupplier nodeList;

        Vector3 target = Vector3.zero;
        Vector3 destiny = Vector3.zero;
        public float wanderDisplacement;
        public float wanderRadious;
        public bool usePathFinding = false;
        bool m_canMove = false;
        MovementState m_state = MovementState.None;
        IPathNode m_pathNodeTarget;
        IPathNode curNodePosition = null;
        int curNodeIdx;
        int nodeIdxSlowingRadious;
        Collider col;
        
        LinkedList<IPathNode> m_curPath;
        IEnumerator<IPathNode> enumerator;

        public event Action OnFinished = delegate { };

        public Vector3 CurDirection { get; private set; }
        public Vector3 Veloctiy { get => m_rb.velocity; }
        public bool IsMoving
        {
            get
            {
                if (m_state == MovementState.Calculating)
                    return true;

                if (m_state == MovementState.Moving || m_state == MovementState.FollowingPath)
                    return true;

                return false;
            }
        }

        float Threshold
        {
            get
            {
                if (m_inventory == null)
                    return m_threshold;

                return stats.MinDistance;
            }
        }
        public PathFinder Finder { get => m_pathFinder; }
        float SlowingRadious => Threshold + m_slowingRadious;

        #region Unity mehthods
        private void Awake()
        {
            m_rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();

            //m_patrolController = gameObject.GetComponent<PatrolController>();
            //m_patrolController.OnFinished += m_patrolController.Execute_Tasks;
        }

        private void FixedUpdate()
        {
            Move();
            
        } 
        #endregion

        public void Initialize(InventoryEquipDecorator inventory, ActionScheduler scheduler, BasicStats stats)
        {
            this.stats = stats;
            this.m_inventory = inventory;
            m_scheduler = scheduler;
        }

        public void SetConnections(INodeListSupplier nodeList)
        {
            m_pathFinder = new PathFinder(nodeList);
            this.nodeList = nodeList;

            m_pathFinder.OnPathCalculated += SetPath;
            curNodePosition = nodeList.FindNearestNode(transform.position);
        }

        public void MoveTo(Vector3 point)
       {
            if (IsMoving || nodeList == null) return;
            m_state = MovementState.Moving;

            try
            {
                curNodePosition ??= nodeList.FindNearestNode(transform.position);

                m_state = MovementState.FollowingPath;

                m_pathFinder.Find_BestRoute<AStar>((curNodePosition, point));
            }
            catch (NullReferenceException)
            {
                m_state = MovementState.None;
                FinishAction();
            }

            //var nearest = nodeList.FindNearestNode(point);

            //if (nearest == null)
            //{
            //    m_state = MovementState.Moving;
            //    return;
            //}

            //if (nodeList.ValidatePosition(point, nearest))
            //{
            //    target = point;
            //    //m_pathFinder.Find_BestRoute<AStar>((transform.position, target)); 
            //}
            //else
            //{
            //    m_state = MovementState.None;
            //    return;
            //    target = nearest.Position;
            //    m_pathFinder.Find_BestRoute<AStar>((transform.position, nearest.Position));
            //}
        }

        public void FollowWithDistance(Movement target, float gap, params Character[] fellows)
        {
            if (IsMoving || nodeList == null) return;

            Vector3 point = SteeringBehaviours.GetFollowPosition(target, this, gap, fellows);
            m_state = MovementState.Calculating;

            try
            {
                curNodePosition ??= nodeList.FindNearestNode(transform.position);

                m_pathFinder.Find_BestRoute<AStar>((curNodePosition, point));
            }
            catch (NullReferenceException)
            {
                m_state = MovementState.None;
                FinishAction();
            }
        }

        public void ChangePositionTo(Transform agent, Vector3 point)
        {
            if (IsMoving) FinishAction();

            curNodePosition = nodeList.FindNearestNode(point);

            if (curNodePosition == null) return;

            agent.position = curNodePosition.Position;
        }


        /// <summary>
        /// Gives access to max speed of agent
        /// </summary>
        /// <returns>float m_speed</returns>
        public float GetSpeed()
        {
            return stats.Speed;
        }

        /// <summary>
        /// Gives access to max velocity of agent
        /// </summary>
        /// <returns>float m_MaxVel</returns>
        public float getMaxVel()
        {
            return m_maxVel = stats.speed;
        }

        /// <summary>
        /// Gives access to steer force of agent
        /// </summary>
        /// <returns>float m_MaxSteerForce</returns>
        public float getMaxSteerForce()
        {
            return m_maxSteerForce;
        }

        public void StartAction()
        {
            if (!m_scheduler.Initilized) return;

            m_scheduler.AddAction(this, ActionPriority.Low);
        }

        public void PauseAction()
        {
            throw new NotImplementedException();
        }

        public void ContinueAction()
        {
            throw new NotImplementedException();
        }

        public void CancelAction()
        {
            m_state = MovementState.None;
        }

        public void StopAction()
        {
            if (m_state == MovementState.FollowingPath)
                FinishAction();
        }

        public void FinishAction()
        {
            m_rb.velocity = new(0, m_rb.velocity.y, 0);
            m_state = MovementState.None;
            m_curPath = null;
            enumerator = null;
            m_pathNodeTarget = null;
            OnFinished?.Invoke();
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
            var pos1 = transform.position + Vector3.down * col.bounds.extents.y;
            var pos2 = target;
            float d = Vector3.Distance(pos1, pos2);
            
            if (d <= SlowingRadious &&
                (m_state == MovementState.Moving ||
                (m_state == MovementState.FollowingPath && curNodeIdx >= nodeIdxSlowingRadious)))
            {
                m_rb.velocity = SteeringBehaviours.Arrival(this, destiny, SlowingRadious, Threshold);
                CurDirection = Vector3.zero;
            }
            else
            if (d <= Threshold &&
                m_state == MovementState.Moving)
            {
                curNodePosition = m_pathNodeTarget;
                FinishAction();
            }
            if (d <= Threshold && m_state == MovementState.FollowingPath)
            {
                if (!GetNextNode())
                    FinishAction();
                else
                    curNodePosition = m_pathNodeTarget;
            }
            else
            {
                CurDirection = m_rb.velocity.normalized;
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
                    curNodeIdx = 0;
                }

                if (enumerator.MoveNext())
                {
                    target = enumerator.Current.Position;
                    m_pathNodeTarget = enumerator.Current;
                    curNodeIdx++;
                    return true;
                }
            }

            return false;
        }

        private void SetPath()
        {
            if (m_pathFinder.BestRoute == null || m_pathFinder.BestRoute.Count == 0)
            {
                FinishAction();
                return;
            }

            m_state = MovementState.FollowingPath;
            m_curPath = m_pathFinder.BestRoute;

            if (!GetNextNode())
            {
                FinishAction();
                return;
            }

            var minNodes = Mathf.Max((int)MathF.Round(SlowingRadious / nodeList.NodeDistance), 0);
            nodeIdxSlowingRadious = m_curPath.Count - minNodes;
            destiny = m_curPath.Last.Value.Position;

            IPathNode lastNode = null;
            foreach (var node in m_pathFinder.BestRoute)
            {
                if (lastNode != null)
                    UnityEngine.Debug.DrawLine(lastNode.Position, node.Position, Color.black, 5);

                lastNode = node;
            }
        }
    }
}
