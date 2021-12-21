using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CloudFine.FlockBox
{
    public partial class LeaderFollowBehavior : GlobalBehavior
    {
        [Tooltip("Distance behind the leader for followers to seek. Prevents followers from crowding the leader.")]
        public float followDistance = 10;
        [Tooltip("Distance from the target point at which followers will begin to slow down.")]
        public float stoppingRadius = 10;
        [Tooltip("Distance ahead of the leader that followers should steer out of the leader's path.")]
        public float clearAheadDistance = 30;
        [Tooltip("Radius around the leader's path within which followers should steer out of the leader's way.")]
        public float clearAheadRadius = 10;

        private Vector3 pointOnLeaderPath_cached;
        public const string leaderIDAttributeName = "leaderID";

        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            HashSet<Agent> leaders = GetFilteredAgents(surroundings, this);

            if (leaders.Count == 0)
            {
                mine.RemoveAgentIntProperty(leaderIDAttributeName);
                steer = Vector3.zero;
                return;
            }

            Agent closestLeader = SeekBehavior.ClosestPursuableTarget(leaders, mine);
            mine.SetAgentIntProperty(leaderIDAttributeName, closestLeader.agentID);

            //check to see if we should clear the way in front of the leader
            float scalar = Vector3.Dot(mine.Position - closestLeader.Position, closestLeader.Forward);
            if (scalar > 0 && scalar < clearAheadDistance)//we are somewhere in front of the leader, potentially in the clear zone ahead of it.
            {
                pointOnLeaderPath_cached = closestLeader.Position + closestLeader.Forward * scalar;
                float insideClearZone = (pointOnLeaderPath_cached - mine.Position).sqrMagnitude / (clearAheadRadius * clearAheadRadius); //0-1 is inside zone, <1 is outside
                if (insideClearZone<=1)
                {
                    steer = (mine.Position - pointOnLeaderPath_cached).normalized * (1f - insideClearZone) * mine.activeSettings.maxForce;
                    return;
                }
            }

            Vector3 desired_velocity = ArriveBehavior.DesiredVelocityForArrival(mine, closestLeader.Position - closestLeader.Forward * followDistance, stoppingRadius);
            steer = desired_velocity - mine.Velocity;
            steer = steer.normalized * Mathf.Min(steer.magnitude, mine.activeSettings.maxForce);
        }


#if UNITY_EDITOR
        public override void DrawPropertyGizmos(SteeringAgent agent, bool drawLabels)
        {
            base.DrawPropertyGizmos(agent, drawLabels);

            Color areaFill = debugColor;
            areaFill.a *= .1f;
            if (agent.HasAgentIntProperty(leaderIDAttributeName))
            {
                int leaderId = agent.GetAgentIntProperty(leaderIDAttributeName);
                Agent leader = Agent.GetAgentById(leaderId);
                if(leader != null)
                {
                    Handles.matrix = leader.transform.localToWorldMatrix;
                    Gizmos.matrix = leader.transform.localToWorldMatrix;
                    areaFill = Color.clear;
                }
            }

            DrawCylinderGizmo(clearAheadRadius, clearAheadDistance);


            Handles.DrawLine(Vector3.zero, Vector3.back * followDistance);

            Gizmos.DrawWireSphere(Vector3.back * followDistance, stoppingRadius);

            if (drawLabels)
            {
                Handles.Label(Vector3.forward * clearAheadDistance, new GUIContent("Clear Ahead Distance"));
                Handles.Label(Vector3.forward * clearAheadDistance + Vector3.up * clearAheadRadius, new GUIContent("Clear Ahead Radius"));

                Handles.Label(Vector3.back * followDistance, new GUIContent("Follow Distance"));
                Handles.Label(Vector3.back * followDistance + Vector3.up * stoppingRadius, new GUIContent("Stopping Radius"));
            }

        }
#endif
    }

    public static class LeaderFollowExtensions
    {
        public static bool HasLeader(this SteeringAgent mine)
        {
            if (!mine.HasAgentIntProperty(LeaderFollowBehavior.leaderIDAttributeName)) return false;
            return mine.GetAgentIntProperty(LeaderFollowBehavior.leaderIDAttributeName) >= 0;
        }

        public static Agent GetLeader(this SteeringAgent mine)
        {
            if (!mine.HasAgentIntProperty(LeaderFollowBehavior.leaderIDAttributeName))
            {
                return null;
            }
            int chosenTargetID = mine.GetAgentIntProperty(LeaderFollowBehavior.leaderIDAttributeName);
            if (chosenTargetID != -1)
            {
                return Agent.GetAgentById(chosenTargetID);
            }
            return null;
        }
    }
}
