using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    [System.Serializable]
    public class PursuitBehavior : SeekBehavior
    {
        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsInfo surroundings)
        {
            if (!mine.HasAttribute(targetIDAttributeName)) mine.SetAttribute(targetIDAttributeName, -1);
            int chosenTargetID = (int)mine.GetAttribute(targetIDAttributeName);

            List<AgentWrapped> allTargets = GetFilteredAgents(surroundings, this);

            if (allTargets.Count > 0)
            {
                if (HasPursuitTarget(mine))
                {
                    DisengagePursuit(mine, chosenTargetID);
                }
                steer = Vector3.zero;
                return;
            }

            AgentWrapped closestTarget = ClosestPursuableTarget(allTargets, mine);

            if (!closestTarget.agent.CanBeCaughtBy(mine))
            {
                if (HasPursuitTarget(mine))
                {
                    DisengagePursuit(mine, chosenTargetID);
                }
                steer = Vector3.zero;
                return;
            }

            if (closestTarget.agent.agentID != chosenTargetID)
            {
                DisengagePursuit(mine, chosenTargetID);
                EngagePursuit(mine, closestTarget.agent);
            }

            Vector3 distance = closestTarget.wrappedPosition - mine.Position;
            float est_timeToIntercept = distance.magnitude / mine.activeSettings.maxSpeed;
            Vector3 predictedInterceptPosition = closestTarget.wrappedPosition + closestTarget.agent.Velocity * est_timeToIntercept;

            AttemptCatch(mine, closestTarget);

            mine.GetSeekVector(out steer, predictedInterceptPosition);
        }
    }
}