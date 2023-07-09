using System.Collections.Generic;
using UnityEngine;

namespace CloudFine.FlockBox
{
    [System.Serializable]
    public partial class SeekBehavior : GlobalRadialSteeringBehavior
    {
        public const string TargetIDAttributeName = "SeekTargetID";
        public const string ManualTargettingAttributeName = "ManualSeekTargetting";

        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            if (!mine.HasAgentIntProperty(TargetIDAttributeName)) mine.SetAgentIntProperty(TargetIDAttributeName, -1);
            int chosenTargetID = mine.GetAgentIntProperty(TargetIDAttributeName);

            if (mine.GetAgentBoolProperty(ManualTargettingAttributeName))
            {
                ManualTarget(mine, chosenTargetID, out steer);
            }
            else
            {
                AutoTarget(mine, surroundings, chosenTargetID, out steer);
            }
        }

        public static Agent FindClosestTarget(HashSet<Agent> nearbyTargets, Agent agent)
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

        protected void AutoTarget(SteeringAgent mine, SurroundingsContainer surroundings, int chosenTargetID, out Vector3 steer)
        {
            HashSet<Agent> allTargets = GetFilteredAgents(surroundings, this);

            if (allTargets.Count == 0)
            {
                if (chosenTargetID != -1)
                {
                    mine.ClearTargetAgent();
                }
                steer = Vector3.zero;
                return;
            }

            Agent closestTarget = FindClosestTarget(allTargets, mine);

            if (!closestTarget || !closestTarget.CanBeCaughtBy(mine))
            {
                if (chosenTargetID != -1)
                {
                    mine.ClearTargetAgent();
                }
                steer = Vector3.zero;
                return;
            }


            if (closestTarget.agentID != chosenTargetID)
            {
                mine.SetAgentIntProperty(TargetIDAttributeName, closestTarget.agentID);
            }

            mine.AttemptCatch(closestTarget);
            steer = GetSteeringVectorForTarget(mine, closestTarget);
        }

        protected void ManualTarget(SteeringAgent mine, int chosenTargetID, out Vector3 steer)
        {
            if (chosenTargetID < 0)
            {
                steer = Vector3.zero;
            }
            else
            {
                Agent target = Agent.GetAgentById(chosenTargetID);
                if (target != null)
                {
                    steer = GetSteeringVectorForTarget(mine, target);
                    mine.AttemptCatch(target);
                }
                else
                {
                    mine.ClearTargetAgent();
                    steer = Vector3.zero;
                }
            }
        }

        protected virtual Vector3 GetSteeringVectorForTarget(SteeringAgent mine, Agent target)
        {
            Vector3 desired_velocity = (target.Position - mine.Position).normalized * mine.activeSettings.maxSpeed;
            Vector3 steer = desired_velocity - mine.Velocity;
            return steer.normalized * Mathf.Min(steer.magnitude, mine.activeSettings.maxForce);
        }
    }

    public static class SeekExtensions
    {
        public static void SetTargetAgent(this SteeringAgent mine, Agent target)
        {
            mine.SetManualAgentTargetting(true);
            mine.SetAgentIntProperty(SeekBehavior.TargetIDAttributeName, target.agentID);
        }

        public static void ClearTargetAgent(this SteeringAgent mine)
        {
            mine.SetAgentIntProperty(SeekBehavior.TargetIDAttributeName, -1);
        }

        public static bool HasTargetAgent(this SteeringAgent mine)
        {
            if (!mine.HasAgentIntProperty(SeekBehavior.TargetIDAttributeName)) return false;
            return mine.GetAgentIntProperty(SeekBehavior.TargetIDAttributeName) >= 0;
        }

        public static Agent GetTargetAgent(this SteeringAgent mine)
        {
            if (!mine.HasAgentIntProperty(SeekBehavior.TargetIDAttributeName))
            {
                return null;
            }
            int chosenTargetID = mine.GetAgentIntProperty(SeekBehavior.TargetIDAttributeName);
            if (chosenTargetID != -1)
            {
                return Agent.GetAgentById(chosenTargetID);
            }
            return null;
        }

        public static void SetManualAgentTargetting(this SteeringAgent mine, bool isManual)
        {
            mine.SetAgentBoolProperty(SeekBehavior.ManualTargettingAttributeName, isManual);
        }
    }
}