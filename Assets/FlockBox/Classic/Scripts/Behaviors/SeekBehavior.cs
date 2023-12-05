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
                    mine.ClearPursuitTarget();
                }
                steer = Vector3.zero;
                return;
            }

            Agent closestTarget = ClosestPursuableTarget(allTargets, mine);

            if (!closestTarget || !closestTarget.CanBeCaughtBy(mine))
            {
                if (chosenTargetID != -1)
                {
                    mine.ClearPursuitTarget();
                }
                steer = Vector3.zero;
                return;
            }


            if (closestTarget.agentID != chosenTargetID)
            {
                mine.SetPusuitTarget(closestTarget);
            }

            mine.AttemptCatch(closestTarget);
            Vector3 desired_velocity = (closestTarget.Position - mine.Position).normalized * mine.activeSettings.maxSpeed;
            steer = desired_velocity - mine.Velocity;
            steer = steer.normalized * Mathf.Min(steer.magnitude, mine.activeSettings.maxForce);
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
        public static void SetPusuitTarget(this SteeringAgent mine, Agent target)
        {
            mine.SetAgentIntProperty(SeekBehavior.targetIDAttributeName, target.agentID);
        }

        public static void ClearPursuitTarget(this SteeringAgent mine)
        {
            mine.SetAgentIntProperty(SeekBehavior.targetIDAttributeName, -1);
        }

        public static bool HasPursuitTarget(this SteeringAgent mine)
        {
            if (!mine.HasAgentIntProperty(SeekBehavior.targetIDAttributeName)) return false;
            return mine.GetAgentIntProperty(SeekBehavior.targetIDAttributeName) >= 0;
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