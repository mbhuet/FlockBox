using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    [System.Serializable]
    public class FleeBehavior : GlobalRadialSteeringBehavior
    {
        public const string fleeAttributeName = "fleeing";

        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            Vector3 fleeMidpoint = Vector3.zero;
            float count = 0;

            foreach (Agent other in GetFilteredAgents(surroundings, this))
            {
                if (WithinEffectiveRadius(mine, other))
                {
                    fleeMidpoint += (other.Position);
                    count++;
                }
            }

            if (count > 0)
            {
                fleeMidpoint /= (count);
                mine.GetSteerVector(out steer, (mine.Position-fleeMidpoint));
                mine.SetAgentProperty(fleeAttributeName, true);
            }
            else
            {
                mine.SetAgentProperty(fleeAttributeName, false);
                steer = Vector3.zero;
            }
        }
    }
}