using Burmuruk.Tesis.Stats;
using UnityEngine;

namespace Burmuruk.Tesis.Movement
{
    [RequireComponent(typeof(Rigidbody))]
    public class Movement : MonoBehaviour
    {
        [SerializeField] float m_maxVel; 

        Rigidbody m_rb;
        StatsManager m_statsManager;

        float m_maxSteerForce;
        public float wanderDisplacement;
        public float wanderRadious;
        public bool usePathFinding = false;

        private void Awake()
        {
            m_rb = GetComponent<Rigidbody>();
        }

        public void MoveTo(Vector3 point)
        {
            SteeringBehaviours.Seek2D(this, point);
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
