using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AvoidanceBehavior : SteeringBehavior {

    public override Vector3 GetSteeringBehaviorVector(SteeringAgent mine, SurroundingsInfo surroundings, float effectiveDistance)
    {
        if (surroundings.obstacles.First == null) return Vector3.zero;
        bool foundObstacleInPath = false;
        float closestHitDistance = float.MaxValue;
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
                    mostThreateningObstacle = obs;
                }
                //Debug.DrawLine(mine.position, closestPoint, Color.blue);
                //Debug.DrawLine(obs.center, closestPoint, Color.green);

            }
        }

        if (!foundObstacleInPath) return Vector3.zero;

       // Debug.DrawRay(mine.position, mine.velocity, Color.yellow);
        Vector3 steer = (mine.position + mine.velocity) - (mostThreateningObstacle.center - mine.position);

        //steer = mine.position - mostThreateningObstacle.center;
        steer = steer.normalized * mine.settings.maxForce;


        //Debug.DrawRay(mine.position, steer, Color.red);
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

