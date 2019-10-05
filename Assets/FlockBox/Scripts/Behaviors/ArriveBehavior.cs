using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    [System.Serializable]
    public class ArriveBehavior : SeekBehavior
    {

        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsInfo surroundings)
        {
            if (!mine.HasAttribute(targetIDAttributeName)) mine.SetAttribute(targetIDAttributeName, -1);
            int chosenTargetID = (int)mine.GetAttribute(targetIDAttributeName);

            List<AgentWrapped> allTargets = GetFilteredAgents(surroundings, this);

            //no targets in neighborhood
            if (allTargets.Count == 0)
            {
                if (chosenTargetID != -1)
                {
                    DisengagePursuit(mine, chosenTargetID);
                }
                steer = Vector3.zero;
                return;
            }

            AgentWrapped closestTarget = ClosestPursuableTarget(allTargets, mine);

            //no pursuable targets nearby
            if (!closestTarget.agent.CanBeCaughtBy(mine)) //double checking because TargetWrapped is a non nullable Struct
            {
                if (chosenTargetID != -1)
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

            AttemptCatch(mine, closestTarget);
            Vector3 desired_velocity =
                (closestTarget.wrappedPosition - mine.Position).normalized
                * Mathf.Lerp(0, mine.activeSettings.maxSpeed, (closestTarget.wrappedPosition - mine.Position).sqrMagnitude / (effectiveRadius * effectiveRadius));
            steer = desired_velocity - mine.Velocity;
            steer = steer.normalized * Mathf.Min(steer.magnitude, mine.activeSettings.maxForce);



        }

        

    }
}