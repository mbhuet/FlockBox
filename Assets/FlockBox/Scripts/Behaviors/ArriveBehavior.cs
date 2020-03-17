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

            HashSet<Agent> allTargets = GetFilteredAgents(surroundings, this);

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

            if (!closestTarget || !closestTarget.CanBeCaughtBy(mine))
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
            Vector3 desired_velocity = DesiredVelocityForArrival(mine, closestTarget.Position, effectiveRadius);
            steer = desired_velocity - mine.Velocity;
            steer = steer.normalized * Mathf.Min(steer.magnitude, mine.activeSettings.maxForce);
        }

        public static Vector3 DesiredVelocityForArrival(SteeringAgent mine, Vector3 arrivePosition, float effectiveRadius)
        {
            return (arrivePosition - mine.Position).normalized
                * Mathf.Lerp(0, mine.activeSettings.maxSpeed, (arrivePosition - mine.Position).sqrMagnitude / (effectiveRadius * effectiveRadius));
        }
    }
}