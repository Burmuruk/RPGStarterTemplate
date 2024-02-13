using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Burmuruk.Tesis.Movement
{
    public class SteeringBehaviours
    {
        /// <summary>
        /// Returns the next position to move the agent towards the target. The y value es ignored.
        /// </summary>
        /// <param name="agent">Object that cointains the agent</param>
        /// <param name="targetPosition">Object that cointains the position</param>
        /// <returns></returns>
        public static Vector3 Seek2D(Movement agent, Vector3 targetPosition)
        {
            Vector3 desiredVel = targetPosition - agent.transform.position;
            return calculateSteer(agent, desiredVel);
        }

        /// <summary>
        /// Returns the next position to move the agent towards the target.
        /// </summary>
        /// <param name="agent">Object that cointains the agent</param>
        /// <param name="targetPosition">Object that cointains the position</param>
        /// <returns></returns>
        public static Vector3 Seek3D(Movement agent, Vector3 targetPosition)
        {
            Vector3 desiredVel = targetPosition - agent.transform.position;
            return calculateSteer3D(agent, desiredVel);
        }

        /// <summary>
        /// Makes the given agent moven to the opposite side of the target.  The y value es ignored.
        /// </summary>
        /// <param name="agent">Object that cointains the agent</param>
        /// <param name="targetPosition">Object that cointains the position</param>
        /// <returns></returns>
        public static Vector3 Flee(Movement agent, Vector3 targetPosition)
        {
            Vector3 desiredVel = agent.transform.position - targetPosition;
            return calculateSteer(agent, desiredVel);
        }

        public static Vector3 Arrival(Movement agent, Vector3 targetPosition, float slowingRadious, float threshold)
        {
            Vector3 newVel = agent.GetComponent<Rigidbody>().velocity;
            float slowingCowficient;

            slowingCowficient = Vector3.Distance(agent.transform.position, targetPosition) is var dis && dis > threshold ? dis / slowingRadious : 0;

            return newVel *= slowingCowficient;
        }

        public static Vector3 Wander(Movement agent)
        {
            Vector3 velCopy = agent.transform.GetComponent<Rigidbody>().velocity;
            velCopy.Normalize();
            velCopy *= agent.wanderDisplacement;
            Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            randomDirection.Normalize();
            randomDirection *= agent.wanderRadious;
            randomDirection += velCopy;
            randomDirection += agent.transform.position;
            return randomDirection;
        }

        public static Vector3 Flee(Movement agent, Transform target)
        {
            Vector3 desiredVel = agent.transform.position - target.position;
            return calculateSteer(agent, desiredVel);
        }

        /// <summary>
        /// Calculates the steering of an object considering 2 dimensions
        /// </summary>
        public static Vector3 calculateSteer(Movement agent, Vector3 desiredVel)
        {
            Rigidbody agentRB = agent.GetComponent<Rigidbody>();
            desiredVel.Normalize();
            desiredVel *= agent.getMaxVel();
            Vector3 steering = desiredVel - agentRB.velocity;
            steering = truncate(steering, agent.getMaxSteerForce());
            steering /= agentRB.mass;
            steering += agentRB.velocity;
            steering = truncate(steering, agent.GetSpeed());
            steering.y = 0;
            return steering;
        }

        /// <summary>
        /// Calculates the steering of an object considering 3 dimensions
        /// </summary>
        public static Vector3 calculateSteer3D(Movement agent, Vector3 desiredVel)
        {
            Rigidbody agentRB = agent.GetComponent<Rigidbody>();
            desiredVel.Normalize();
            desiredVel *= agent.getMaxVel();
            Vector3 steering = desiredVel - agentRB.velocity;
            steering = truncate(steering, agent.getMaxSteerForce());
            steering /= agentRB.mass;
            steering += agentRB.velocity;
            steering = truncate(steering, agent.GetSpeed());
            return steering;
        }


        public static void LookAt(Transform agent, Vector3 direction)
        {
            agent.transform.LookAt(agent.position + direction);
        }

        private static Vector3 truncate(Vector3 vector, float maxValue)
        {
            if (vector.magnitude <= maxValue)
            {
                return vector;
            }
            vector.Normalize();
            return vector *= maxValue;
        }
    } 
}

