using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CloudFine.FlockBox
{
    [System.Serializable]
    public partial class AvoidanceBehavior : ForecastSteeringBehavior
    {
        [Tooltip("Extra clearance space to strive for when avoiding obstacles.")]
        public float clearance;

        RaycastHit closestHit;
        RaycastHit hit;
        Agent mostImmediateObstacle;
        Vector3 edgePoint;
        Vector3 normal;
        Vector3 closestPoint;

        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            HashSet<Agent> obstacles = GetFilteredAgents(surroundings, this);
            if (obstacles.Count == 0)
            {
                steer = Vector3.zero;
                return;
            }

            Ray myRay = new Ray(mine.Position, mine.Forward);
            float rayDist = surroundings.lookAheadSeconds * mine.Velocity.magnitude;
            bool foundObstacleInPath = false;
            foreach (Agent obstacle in obstacles)
            {
                if (obstacle.RaycastToShape(myRay, mine.shape.radius + clearance, rayDist, out hit))
                {
                    if (!foundObstacleInPath || hit.distance < closestHit.distance)
                    {
                        closestHit = hit;
                        mostImmediateObstacle = obstacle;
                    }
                    foundObstacleInPath = true;
                }
            }

            if (!foundObstacleInPath)
            {
                steer = Vector3.zero;
                return;
            }
            mostImmediateObstacle.FindNormalToSteerAwayFromShape(myRay, closestHit, mine.shape.radius, ref normal);
            steer = normal;
            steer = steer.normalized * mine.activeSettings.maxForce;
            steer *= (1f - (closestHit.distance / rayDist));
        }



#if UNITY_EDITOR
        protected override void DrawForecastPerceptionGizmo(SteeringAgent agent, float distance)
        {
            DrawCylinderGizmo(agent.shape.radius + clearance, distance);
            Handles.DrawWireDisc(Vector3.forward * distance, Vector3.forward, agent.shape.radius);
            Handles.Label(Vector3.forward * distance + Vector3.up * agent.shape.radius, new GUIContent("Agent Radius"));
            Handles.Label(Vector3.forward * distance + Vector3.up * (agent.shape.radius + clearance), new GUIContent("Clearance"));
        }
#endif
    }
}