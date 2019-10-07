using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    [System.Serializable]
    public class AvoidanceBehavior : ForecastSteeringBehavior
    {
        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsInfo surroundings)
        {
            List<Agent> obstacles = GetFilteredAgents(surroundings, this);
            if (obstacles.Count == 0)
            {
                steer = Vector3.zero;
                return;
            }
            bool foundObstacleInPath = false;
            float closestHitDistance = float.MaxValue;
            Vector3 closestHitPoint = Vector3.zero;
            Agent mostThreateningObstacle = obstacles[0];

            foreach (Agent obs_wrapped in obstacles)
            {
                Vector3 closestPoint = ClosestPointPathToObstacle(mine, obs_wrapped);
                if (Vector3.Distance(closestPoint, obs_wrapped.Position) < obs_wrapped.Radius)
                {
                    //found obstacle directly in path
                    foundObstacleInPath = true;

                    float distanceToClosestPoint = Vector3.Distance(closestPoint, mine.Position);
                    if (distanceToClosestPoint < closestHitDistance)
                    {
                        closestHitDistance = distanceToClosestPoint;
                        closestHitPoint = closestPoint;
                        mostThreateningObstacle = obs_wrapped;
                    }

                }
            }

            if (!foundObstacleInPath)
            {
                steer = Vector3.zero;
                return;
            }
            float distanceToObstacleEdge = Mathf.Max(Vector3.Distance(mine.Position, mostThreateningObstacle.Position) - mostThreateningObstacle.Radius, 1);
            steer = closestHitPoint - mostThreateningObstacle.Position;
            steer = steer.normalized * mine.activeSettings.maxForce;
        }

        Vector3 ClosestPointPathToObstacle(SteeringAgent mine, Agent obstacle)
        {
            Vector3 agentPos = mine.Position;
            Vector3 agentToObstacle = obstacle.Position - agentPos;
            Vector3 projection = Vector3.Project(agentToObstacle, mine.Velocity.normalized);
            if (projection.normalized == mine.Velocity.normalized)
                return agentPos + projection;
            else return agentPos;
        }
    }
}