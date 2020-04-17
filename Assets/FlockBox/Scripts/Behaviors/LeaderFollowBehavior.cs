using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CloudFine
{
    public class LeaderFollowBehavior : GlobalBehavior
    {
        public float followDistance = 10;
        public float stoppingRadius = 10;
        public float clearAheadDistance = 30;
        public float clearAheadRadius = 10;

        private Vector3 pointOnLeaderPath_cached;
        public const string leaderIDAttributeName = "leaderID";

        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            HashSet<Agent> leaders = GetFilteredAgents(surroundings, this);

            if (leaders.Count == 0)
            {
                mine.RemoveAttribute(leaderIDAttributeName);
                steer = Vector3.zero;
                return;
            }

            Agent closestLeader = SeekBehavior.ClosestPursuableTarget(leaders, mine);
            mine.SetAttribute(leaderIDAttributeName, closestLeader.agentID);

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
        public override void DrawPerceptionGizmo(SteeringAgent agent)
        {
            base.DrawPerceptionGizmo(agent);

            Color areaFill = debugColor;
            areaFill.a *= .1f;
            if (agent.HasAttribute(leaderIDAttributeName))
            {
                int leaderId = (int)agent.GetAttribute(leaderIDAttributeName);
                Agent leader = Agent.GetAgentById(leaderId);
                if(leader != null)
                {
                    Handles.matrix = leader.transform.localToWorldMatrix;
                    Gizmos.matrix = leader.transform.localToWorldMatrix;
                    areaFill = Color.clear;
                }
            }
            Handles.color = debugColor;

            Handles.DrawLine(Vector3.zero, Vector3.forward * clearAheadDistance);
            Handles.DrawWireDisc(Vector3.forward * clearAheadDistance, Vector3.forward, clearAheadRadius);
            Handles.DrawWireDisc(Vector3.zero, Vector3.forward, clearAheadRadius);

            Vector3[] verts = new Vector3[]
            {
                Vector3.left * clearAheadRadius,
                Vector3.left * clearAheadRadius + Vector3.forward * clearAheadDistance,
                Vector3.right * clearAheadRadius + Vector3.forward * clearAheadDistance,
                Vector3.right *clearAheadRadius
            };

            Handles.DrawSolidRectangleWithOutline(verts, areaFill, debugColor);

            Handles.DrawLine(Vector3.up * clearAheadRadius,
                Vector3.up * clearAheadRadius + Vector3.forward * clearAheadDistance);

            Handles.DrawLine(Vector3.down * clearAheadRadius + Vector3.forward * clearAheadDistance,
                Vector3.down * clearAheadRadius);


            Handles.DrawLine(Vector3.zero, Vector3.back * followDistance);
            Gizmos.DrawSphere(Vector3.back * followDistance, 1);

            Gizmos.DrawWireSphere(Vector3.back * followDistance, stoppingRadius);
        }
#endif
    }
}
