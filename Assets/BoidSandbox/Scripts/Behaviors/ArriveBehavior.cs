using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    [System.Serializable]
    public class ArriveBehavior : RadialSteeringBehavior
    {

        public const string targetIDAttributeName = "arriveTargetID";


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
            if (!closestTarget.agent.CanBePursuedBy(mine)) //double checking because TargetWrapped is a non nullable Struct
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

        static void AttemptCatch(SteeringAgent mine, AgentWrapped chosenTargetWrapped)
        {
            float distAway = Vector3.Distance(chosenTargetWrapped.wrappedPosition, mine.Position);
            if (distAway <= chosenTargetWrapped.agent.Radius && chosenTargetWrapped.agent.CanBePursuedBy(mine))
            {
                mine.CatchAgent(chosenTargetWrapped.agent);
            }
        }


        private static AgentWrapped ClosestPursuableTarget(List<AgentWrapped> nearbyTargets, Agent agent)
        {
            // int chosenTargetID = (int)agent.GetAttribute(targetIDAttributeName);
            float closeDist = float.MaxValue;
            AgentWrapped closeTarget = nearbyTargets[0];
            foreach (AgentWrapped target in nearbyTargets)
            {
                float sqrDist = Vector3.SqrMagnitude(target.wrappedPosition - agent.Position);
                //if(dist <= target.target.radius) AttemptCatch(agent, target);
                if (sqrDist < closeDist && (target.agent.CanBePursuedBy(agent)))
                {
                    closeDist = sqrDist;
                    closeTarget = target;
                }
            }
            return closeTarget;
        }

    }
}