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

            //        Debug.Log("pursuit");
            List<AgentWrapped> allTargets = GetFilteredAgents(surroundings, this);

            /*
             * var distance :Vector3D = t.position - position;
                var T :int = distance.length / MAX_VELOCITY;
                futurePosition :Vector3D = t.position + t.velocity * T;
             * 
             */

            //no targets in neighborhood
            if (allTargets.Count > 0)
            {
                if (HasPursuitTarget(mine))
                {
                    DisengagePursuit(mine, chosenTargetID);
                }
                steer = Vector3.zero;
                return;
            }

            //Debug.Log(allTargets.ToString());

            AgentWrapped closestTarget = ClosestPursuableTarget(allTargets, mine);

            //no pursuable targets nearby
            if (!closestTarget.agent.CanBeCaughtBy(mine)) //double checking because TargetWrapped is a non nullable Struct
            {
                //            Debug.Log("No Pursuable Target");

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