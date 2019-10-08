using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    [System.Serializable]
    public class ArriveBehavior : SeekBehavior
    {
        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            if (!mine.HasAttribute(targetIDAttributeName)) mine.SetAttribute(targetIDAttributeName, -1);
            int chosenTargetID = (int)mine.GetAttribute(targetIDAttributeName);

            List<Agent> allTargets = GetFilteredAgents(surroundings, this);

            if (allTargets.Count == 0)
            {
                if (chosenTargetID != -1)
                {
                    DisengagePursuit(mine, chosenTargetID);
                }
                steer = Vector3.zero;
                return;
            }

            Agent closestTarget = ClosestPursuableTarget(allTargets, mine);

            if (!closestTarget.CanBeCaughtBy(mine))
            {
                if (chosenTargetID != -1)
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

            AttemptCatch(mine, closestTarget);
            Vector3 desired_velocity =
                (closestTarget.Position - mine.Position).normalized
                * Mathf.Lerp(0, mine.activeSettings.maxSpeed, (closestTarget.Position - mine.Position).sqrMagnitude / (effectiveRadius * effectiveRadius));
            steer = desired_velocity - mine.Velocity;
            steer = steer.normalized * Mathf.Min(steer.magnitude, mine.activeSettings.maxForce);
        }
    }
}