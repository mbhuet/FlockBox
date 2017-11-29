using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AvoidanceBehavior : SteeringBehavior {

    public override Vector3 GetSteeringBehaviorVector(SteeringAgent mine, SurroundingsInfo surroundings, float effectiveDistance)
    {
        if (surroundings.obstacles.First == null) return Vector3.zero;
        bool foundObstacleInPath = false;
        float closestHitDistance = float.MaxValue;
        Vector3 closestHitPoint = Vector3.zero;
        Obstacle mostThreateningObstacle = surroundings.obstacles.First.Value;

        foreach (Obstacle obs in surroundings.obstacles)
        {
            Vector3 closestPoint = ClosestPointPathToObstacle(mine, obs);
            if (Vector3.Distance(closestPoint, obs.center) < obs.radius)
            {
                //found obstacle directly in path
                foundObstacleInPath = true;

                float distanceToClosestPoint = Vector3.Distance(closestPoint, mine.position);
                if (distanceToClosestPoint < closestHitDistance)
                {
                    closestHitDistance = distanceToClosestPoint;
                    closestHitPoint = closestPoint;
                    mostThreateningObstacle = obs;
                }

            }
        }

        if (!foundObstacleInPath) return Vector3.zero;
        float distanceToObstacleEdge = Mathf.Max(Vector3.Distance(mine.position, mostThreateningObstacle.center) - mostThreateningObstacle.radius, 1);
        if (distanceToObstacleEdge > effectiveDistance) return Vector3.zero;
        Vector3 steer = closestHitPoint - mostThreateningObstacle.center;

        steer = steer.normalized * mine.settings.maxForce;
        return steer;
    }


    Vector3 ClosestPointPathToObstacle(SteeringAgent agent, Obstacle obstacle)
    {
        Vector3 agentPos = agent.position;
        Vector3 agentToObstacle = obstacle.center - agentPos;
        Vector3 projection = Vector3.Project(agentToObstacle, agent.velocity.normalized);
        if (projection.normalized == agent.velocity.normalized)
            return agentPos + projection;
        else return agentPos;
    }
}

