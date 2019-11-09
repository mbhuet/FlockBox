using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    public class LeaderFollowBehavior : GlobalBehavior
    {
        public float followDistance = 10;
        public float stoppingRadius = 10;
        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            List<Agent> leaders = GetFilteredAgents(surroundings, this);

            if (leaders.Count == 0)
            {
                steer = Vector3.zero;
                return;
            }

            Agent closestLeader = SeekBehavior.ClosestPursuableTarget(leaders, mine);
            Vector3 desired_velocity = ArriveBehavior.DesiredVelocityForArrival(mine, closestLeader.Position - closestLeader.Forward.normalized * followDistance, stoppingRadius);
            steer = desired_velocity - mine.Velocity;
            steer = steer.normalized * Mathf.Min(steer.magnitude, mine.activeSettings.maxForce);
        }
    }
}
