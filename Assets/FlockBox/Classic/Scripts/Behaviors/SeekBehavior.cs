using System.Collections.Generic;
using UnityEngine;

namespace CloudFine.FlockBox
{
    [System.Serializable]
    public partial class SeekBehavior : GlobalRadialSteeringBehavior
    {
        public const string targetIDAttributeName = "seekTargetID";

        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            if (!mine.HasAgentIntProperty(targetIDAttributeName)) mine.SetAgentIntProperty(targetIDAttributeName, -1);
            int chosenTargetID = mine.GetAgentIntProperty(targetIDAttributeName);

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

            mine.AttemptCatch(closestTarget);
            Vector3 desired_velocity = (closestTarget.Position - mine.Position).normalized * mine.activeSettings.maxSpeed;
            steer = desired_velocity - mine.Velocity;
            steer = steer.normalized * Mathf.Min(steer.magnitude, mine.activeSettings.maxForce);
        }

        protected static void EngagePursuit(SteeringAgent mine, Agent target)
        {
            mine.SetAgentIntProperty(SeekBehavior.targetIDAttributeName, target.agentID);
        }

        protected static void DisengagePursuit(SteeringAgent mine, int targetID)
        {
            mine.SetAgentIntProperty(SeekBehavior.targetIDAttributeName, -1);
        }

        public static Agent ClosestPursuableTarget(HashSet<Agent> nearbyTargets, Agent agent)
        {
            if (nearbyTargets.Count == 0) return null;
            float closeDist = float.MaxValue;
            Agent closeTarget = null;
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

    public static class SeekExtensions
    {
        public static bool HasPursuitTarget(this SteeringAgent mine)
        {
            if (!mine.HasAgentIntProperty(SeekBehavior.targetIDAttributeName)) return false;
            return mine.GetAgentIntProperty(SeekBehavior.targetIDAttributeName) >= 0;
        }

        public static void AttemptCatch(this SteeringAgent mine, Agent target)
        {
            if (mine.Overlaps(target))
            {
                mine.CatchAgent(target);
            }
        }

        public static Agent GetTarget(this SteeringAgent mine)
        {
            if (!mine.HasAgentIntProperty(SeekBehavior.targetIDAttributeName))
            {
                return null;
            }
            int chosenTargetID = mine.GetAgentIntProperty(SeekBehavior.targetIDAttributeName);
            if (chosenTargetID != -1)
            {
                return Agent.GetAgentById(chosenTargetID);
            }
            return null;
        }
    }
}