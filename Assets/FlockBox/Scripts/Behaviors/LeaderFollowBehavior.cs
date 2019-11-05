using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    public class LeaderFollowBehavior : GlobalBehavior
    {
        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            List<Agent> leaders = GetFilteredAgents(surroundings, this);

            if (leaders.Count == 0)
            {
                steer = Vector3.zero;
                return;
            }
            mine.GetSeekVector(out steer, leaders[0].Position);
        }
    }
}
