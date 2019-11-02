using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    [System.Serializable]
    public class PathFollowBehavior : ForecastSteeringBehavior
    {

        private RaycastHit hit;

        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            List<Agent> paths = GetFilteredAgents(surroundings, this);
            if (paths.Count == 0)
            {
                steer = Vector3.zero;
                return;
            }

            Ray myRay = new Ray(mine.Position, mine.Forward);
            foreach (Agent path in paths)
            {
                path.RaycastToShape(myRay, lookAheadSeconds * mine.Velocity.magnitude, out hit);
            }
            Vector3 target = Vector3.zero;
            mine.GetSeekVector(out steer, target);
        }
    }
}