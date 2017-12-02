using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AvoidanceBehavior : SteeringBehavior {

    public override Vector3 GetSteeringBehaviorVector(SteeringAgent mine, SurroundingsInfo surroundings)
    {
        if (surroundings.obstacles.First == null) return Vector3.zero;
        bool foundObstacleInPath = false;
        float closestHitDistance = float.MaxValue;
        Vector3 closestHitPoint = Vector3.zero;
        Obstacle mostThreateningObstacle = surroundings.obstacles.First.Value.obstacle;

        foreach (ObstacleWrapped obs_wrapped in surroundings.obstacles)
        {
            Obstacle obstacle = obs_wrapped.obstacle;
            Vector3 closestPoint = ClosestPointPathToObstacle(mine, obs_wrapped);
            if (Vector3.Distance(closestPoint, obs_wrapped.wrappedCenter) < obstacle.radius)
            {
                //found obstacle directly in path
                foundObstacleInPath = true;

                float distanceToClosestPoint = Vector3.Distance(closestPoint, mine.position);
                if (distanceToClosestPoint < closestHitDistance)
                {
                    closestHitDistance = distanceToClosestPoint;
                    closestHitPoint = closestPoint;
                    mostThreateningObstacle = obstacle;
                }

            }
        }

        if (!foundObstacleInPath) return Vector3.zero;
        float distanceToObstacleEdge = Mathf.Max(Vector3.Distance(mine.position, mostThreateningObstacle.center) - mostThreateningObstacle.radius, 1);
        if (distanceToObstacleEdge > effectiveRadius) return Vector3.zero;
        Vector3 steer = closestHitPoint - mostThreateningObstacle.center;

        steer = steer.normalized * mine.settings.maxForce;
        return steer * weight;
    }


    Vector3 ClosestPointPathToObstacle(SteeringAgent agent, ObstacleWrapped obstacle)
    {
        Vector3 agentPos = agent.position;
        Vector3 agentToObstacle = obstacle.wrappedCenter - agentPos;
        Vector3 projection = Vector3.Project(agentToObstacle, agent.velocity.normalized);
        if (projection.normalized == agent.velocity.normalized)
            return agentPos + projection;
        else return agentPos;
    }
}

