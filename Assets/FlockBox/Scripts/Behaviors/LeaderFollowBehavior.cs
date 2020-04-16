using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            Color c = debugColor;
            UnityEditor.Handles.color = c;
            c.a = c.a * .1f;

            UnityEditor.Handles.DrawLine(Vector3.zero, Vector3.forward * clearAheadDistance);
            UnityEditor.Handles.DrawWireDisc(Vector3.forward * clearAheadDistance, Vector3.forward, clearAheadRadius);
            UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.forward, clearAheadRadius);

            Vector3[] verts = new Vector3[]
            {
                Vector3.left * clearAheadRadius,
                Vector3.left * clearAheadRadius + Vector3.forward * clearAheadDistance,
                Vector3.right * clearAheadRadius + Vector3.forward * clearAheadDistance,
                Vector3.right *clearAheadRadius
            };

            UnityEditor.Handles.DrawSolidRectangleWithOutline(verts, c, debugColor);

            UnityEditor.Handles.DrawLine(Vector3.zero, Vector3.back * followDistance);
            UnityEditor.Handles.DrawSolidDisc(Vector3.back * followDistance, Vector3.up, 1);

            UnityEditor.Handles.DrawWireDisc(Vector3.back * followDistance, Vector3.right, stoppingRadius);
            UnityEditor.Handles.DrawWireDisc(Vector3.back * followDistance, Vector3.up, stoppingRadius);
        }
#endif
    }
}
