using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    [System.Serializable]
    public class CohesionBehavior : RadialSteeringBehavior
    {
        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsInfo surroundings)
        {
            Vector3 sum = Vector3.zero;
            float count = 0;
            foreach (AgentWrapped other in GetFilteredAgents(surroundings, this))
            {
                if (WithinEffectiveRadius(mine, other))
                {
                    sum += (other.wrappedPosition);
                    count ++;
                }
            }
            if (count > 0)
            {
                sum /= (count);
                mine.GetSeekVector(out steer, sum);
            }
            else
            {
                steer = Vector3.zero;
            }
        }
    }
}