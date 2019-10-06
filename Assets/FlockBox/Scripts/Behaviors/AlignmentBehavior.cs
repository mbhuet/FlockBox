using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    [System.Serializable]
    public class AlignmentBehavior : RadialSteeringBehavior
    {
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