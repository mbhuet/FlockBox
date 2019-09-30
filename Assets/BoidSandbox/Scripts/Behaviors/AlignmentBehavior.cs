using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    [System.Serializable]
    public class AlignmentBehavior : SteeringBehavior
    {

        // Alignment
        // For every nearby boid in the system, calculate the average velocity
        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsInfo surroundings)
        {
            Vector3 sum = Vector3.zero;
            int count = 0;
            foreach (AgentWrapped other in GetFilteredAgents(surroundings, this))
            {
                if (WithinEffectiveRadius(mine, other))
                {
                    float modFactor = 1;
                    sum += (other.agent.Velocity) * modFactor;
                    count++;
                }
            }
            if (count > 0)
            {
                sum /= ((float)count);
                // First two lines of code below could be condensed with new Vector3 setMag() method
                // Not using this method until Processing.js catches up
                // sum.setMag(maxspeed);

                // Implement Reynolds: Steering = Desired - Velocity
                sum.Normalize();
                sum *= (mine.activeSettings.maxSpeed);
                steer = sum - mine.Velocity;
                steer = steer.normalized * Mathf.Min(steer.magnitude, mine.activeSettings.maxForce);

            }
            else
            {
                steer = Vector3.zero;
            }
        }
    }
}