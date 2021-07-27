using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CloudFine.FlockBox
{
    [System.Serializable]
    public class ArriveBehavior : SeekBehavior
    {
        [Tooltip("Distance at which brake force will be applied to bring the agent to a stop.")]
        public float stoppingDistance = 10;

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
            Vector3 desired_velocity = DesiredVelocityForArrival(mine, closestTarget.Position, stoppingDistance);
            steer = desired_velocity - mine.Velocity;
            steer = steer.normalized * Mathf.Min(steer.magnitude, mine.activeSettings.maxForce);
        }

        public static Vector3 DesiredVelocityForArrival(SteeringAgent mine, Vector3 arrivePosition, float stopRadius)
        {
            return (arrivePosition - mine.Position).normalized
                * Mathf.Lerp(0, mine.activeSettings.maxSpeed, (arrivePosition - mine.Position).sqrMagnitude / (stopRadius * stopRadius));
        }

#if UNITY_EDITOR
        public override void DrawPropertyGizmos(SteeringAgent agent, bool drawLabels)
        {
            base.DrawPropertyGizmos(agent, drawLabels);

            Handles.color = debugColor;
            Handles.DrawWireDisc(Vector3.zero, Vector3.up, stoppingDistance);
            
            if (drawLabels)
            {
                Handles.Label(Vector3.forward * stoppingDistance, new GUIContent("Stopping Distance"));
            }
        }
#endif
    }
}