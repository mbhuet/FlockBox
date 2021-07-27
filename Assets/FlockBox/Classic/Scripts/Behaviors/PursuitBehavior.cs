using System.Collections.Generic;
using UnityEngine;

namespace CloudFine.FlockBox
{
    [System.Serializable]
    public class PursuitBehavior : SeekBehavior
    {
        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            if (!mine.HasAgentIntProperty(targetIDAttributeName)) mine.SetAgentIntProperty(targetIDAttributeName, -1);
            int chosenTargetID = mine.GetAgentIntProperty(targetIDAttributeName);

            HashSet<Agent> allTargets = GetFilteredAgents(surroundings, this);

            if (allTargets.Count == 0)
            {
                if (mine.HasPursuitTarget())
                {
                    DisengagePursuit(mine, chosenTargetID);
                }
                steer = Vector3.zero;
                return;
            }

            Agent closestTarget = ClosestPursuableTarget(allTargets, mine);

            if (!closestTarget || !closestTarget.CanBeCaughtBy(mine))
            {
                if (mine.HasPursuitTarget())
                {
                    DisengagePursuit(mine, chosenTargetID);
                }
                steer = Vector3.zero;
                return;
            }

            if (closestTarget.agentID != chosenTargetID)
            {
                DisengagePursuit(mine, chosenTargetID);
                EngagePursuit(mine, closestTarget);
            }

            Vector3 distance = closestTarget.Position - mine.Position;
            float est_timeToIntercept = distance.magnitude / mine.activeSettings.maxSpeed;
            Vector3 predictedInterceptPosition = closestTarget.Position + closestTarget.Velocity * est_timeToIntercept;

            mine.AttemptCatch(closestTarget);

            mine.GetSeekVector(out steer, predictedInterceptPosition);
        }
    }
}