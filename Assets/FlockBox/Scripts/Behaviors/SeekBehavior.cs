using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    [System.Serializable]
    public class SeekBehavior : RadialSteeringBehavior
    {
        public const string targetIDAttributeName = "seekTargetID";

        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsInfo surroundings)
        {
            if (!mine.HasAttribute(targetIDAttributeName)) mine.SetAttribute(targetIDAttributeName, -1);
            int chosenTargetID = (int)mine.GetAttribute(targetIDAttributeName);

            List<AgentWrapped> allTargets = GetFilteredAgents(surroundings, this);

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

            if (!closestTarget.agent.CanBeCaughtBy(mine))
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
            Vector3 desired_velocity = (closestTarget.wrappedPosition - mine.Position).normalized * mine.activeSettings.maxSpeed;
            steer = desired_velocity - mine.Velocity;
            steer = steer.normalized * Mathf.Min(steer.magnitude, mine.activeSettings.maxForce);
        }

        public static bool HasPursuitTarget(SteeringAgent mine)
        {
            if (!mine.HasAttribute(targetIDAttributeName)) return false;
            return (int)mine.GetAttribute(targetIDAttributeName) >= 0;
        }

        protected static void EngagePursuit(SteeringAgent mine, Agent target)
        {
            mine.SetAttribute(targetIDAttributeName, target.agentID);
        }

        protected static void DisengagePursuit(SteeringAgent mine, int targetID)
        {
            mine.SetAttribute(targetIDAttributeName, -1);
        }

        protected static void AttemptCatch(SteeringAgent mine, AgentWrapped chosenTargetWrapped)
        {
            float distAway = Vector3.Distance(chosenTargetWrapped.wrappedPosition, mine.Position);
            if (distAway <= (chosenTargetWrapped.agent.Radius + mine.Radius) && chosenTargetWrapped.agent.CanBeCaughtBy(mine))
            {
                mine.CatchAgent(chosenTargetWrapped.agent);
            }
        }

        protected static AgentWrapped ClosestPursuableTarget(List<AgentWrapped> nearbyTargets, Agent agent)
        {
            float closeDist = float.MaxValue;
            AgentWrapped closeTarget = nearbyTargets[0];
            foreach (AgentWrapped target in nearbyTargets)
            {
                float sqrDist = Vector3.SqrMagnitude(target.wrappedPosition - agent.Position);
                if (sqrDist < closeDist && (target.agent.CanBeCaughtBy(agent)))
                {
                    closeDist = sqrDist;
                    closeTarget = target;
                }
            }
            return closeTarget;
        }
    }
}