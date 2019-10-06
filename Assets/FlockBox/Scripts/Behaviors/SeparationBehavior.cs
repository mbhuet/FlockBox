using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    [System.Serializable]
    public class SeparationBehavior : RadialSteeringBehavior
    {
        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsInfo surroundings)
        {
            steer = Vector3.zero;
            int count = 0;
            foreach (AgentWrapped other in GetFilteredAgents(surroundings, this))
            {

                if (WithinEffectiveRadius(mine, other))
                {
                    Vector3 diff = mine.Position - other.wrappedPosition;
                    if (diff.sqrMagnitude < .001f) diff = UnityEngine.Random.insideUnitCircle * .01f;
                    steer += (diff.normalized / diff.magnitude);
                    count++;
                }
            }
            if (count > 0)
            {
                steer /= ((float)count);
            }

            if (steer.magnitude > 0)
            {
                steer = steer.normalized * (mine.activeSettings.maxSpeed);
                steer -= (mine.Velocity);
                steer = steer.normalized * Mathf.Min(steer.magnitude, mine.activeSettings.maxForce);
            }
        }
    }
}