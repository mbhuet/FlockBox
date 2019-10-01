using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    [System.Serializable]
    public class PursuitBehavior : RadialSteeringBehavior
    {
        public const string targetIDAttributeName = "seekTargetID";


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

            AgentWrapped closestTarget = ClosestTarget(allTargets, mine);

            //no pursuable targets nearby
            if (!closestTarget.agent.CanBePursuedBy(mine)) //double checking because TargetWrapped is a non nullable Struct
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

        public static bool HasPursuitTarget(SteeringAgent mine)
        {
            if (!mine.HasAttribute(targetIDAttributeName)) return false;
            return (int)mine.GetAttribute(targetIDAttributeName) >= 0;

        }

        static void EngagePursuit(SteeringAgent mine, Agent target)
        {
            mine.SetAttribute(targetIDAttributeName, target.agentID);
            target.InformOfPursuit(true, mine);
        }

        static void DisengagePursuit(SteeringAgent mine, int targetID)
        {
            mine.SetAttribute(targetIDAttributeName, -1);
            Agent.InformOfPursuit(false, mine, targetID);

        }

        static void AttemptCatch(Agent mine, AgentWrapped chosenQuaryWrapped)
        {
            float distAway = Vector3.Distance(chosenQuaryWrapped.wrappedPosition, mine.Position);
            if (distAway <= (chosenQuaryWrapped.agent.Radius + mine.Radius) && chosenQuaryWrapped.agent.CanBePursuedBy(mine))
            {
                mine.CatchAgent(chosenQuaryWrapped.agent);
                //            Debug.Log(chosenQuaryWrapped.agent.name + " successful catch by " + mine.name);
            }
        }


        private static AgentWrapped ClosestTarget(List<AgentWrapped> nearbyTargets, Agent agent)
        {
            int chosenTargetID = (int)agent.GetAttribute(targetIDAttributeName);

            float closeDist = float.MaxValue;
            AgentWrapped closeTarget = nearbyTargets[0];
            foreach (AgentWrapped target in nearbyTargets)
            {
                //Debug.DrawLine(agent.position, target.wrappedPosition, target.agent.CanBePursuedBy(agent)? Color.blue : Color.yellow);
                float dist = (target.wrappedPosition - agent.Position).sqrMagnitude;
                //if(dist <= target.target.radius) AttemptCatch(agent, target);
                if (dist < closeDist && target.agent.CanBePursuedBy(agent))
                {
                    closeDist = dist;
                    closeTarget = target;
                }
            }
            return closeTarget;
        }

    }
}