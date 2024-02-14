using Burmuruk.Tesis.Stats;
using UnityEngine;

namespace Burmuruk.Tesis.Movement
{
    [RequireComponent(typeof(Rigidbody))]
    public class Movement : MonoBehaviour
    {
        [SerializeField] float m_maxVel; 
        [SerializeField] float m_maxSteerForce;
        [SerializeField] float m_threshold;

        Rigidbody m_rb;
        StatsManager m_statsManager;
        Inventary m_inventary;

        public float wanderDisplacement;
        public float wanderRadious;
        public bool usePathFinding = false;

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
        }

        public void MoveTo(Vector3 point)
        {
            if (Vector3.Distance(transform.position, point) > SlowingRadious)
            {
                m_rb.velocity = SteeringBehaviours.Seek2D(this, point); 
            }
            else
            {
                m_rb.velocity = SteeringBehaviours.Arrival(this, point, SlowingRadious, m_threshold);
            }
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
    }
}
