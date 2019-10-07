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
            Vector3 desired_velocity = (closestTarget.Position - mine.Position).normalized * mine.activeSettings.maxSpeed;
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

        protected static void AttemptCatch(SteeringAgent mine, Agent chosenTargetWrapped)
        {
            float distAway = Vector3.Distance(chosenTargetWrapped.Position, mine.Position);
            if (distAway <= (chosenTargetWrapped.Radius + mine.Radius) && chosenTargetWrapped.CanBeCaughtBy(mine))
            {
                mine.CatchAgent(chosenTargetWrapped);
            }
        }

        protected static Agent ClosestPursuableTarget(List<Agent> nearbyTargets, Agent agent)
        {
            float closeDist = float.MaxValue;
            Agent closeTarget = nearbyTargets[0];
            foreach (Agent target in nearbyTargets)
            {
                float sqrDist = Vector3.SqrMagnitude(target.Position - agent.Position);
                if (sqrDist < closeDist && (target.CanBeCaughtBy(agent)))
                {
                    closeDist = sqrDist;
                    closeTarget = target;
                }
            }
            return closeTarget;
        }
    }
}