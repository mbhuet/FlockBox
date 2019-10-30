using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    [System.Serializable]
    public class AvoidanceBehavior : ForecastSteeringBehavior
    {
        float distanceToClosestPoint;
        bool foundObstacleInPath = false;
        RaycastHit closestHit;
        RaycastHit hit;

        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            List<Agent> obstacles = GetFilteredAgents(surroundings, this);
            if (obstacles.Count == 0)
            {
                steer = Vector3.zero;
                return;
            }

            Ray myRay = new Ray(mine.Position, mine.Forward);
            foundObstacleInPath = false;

            foreach (Agent obstacle in obstacles)
            {
                if(obstacle.RaycastToShape(myRay, out hit))
                {
                    if (!foundObstacleInPath || hit.distance < closestHit.distance)
                    {
                        closestHit = hit;
                    }
                    foundObstacleInPath = true;      
                }
            }

            if (!foundObstacleInPath)
            {
                steer = Vector3.zero;
                return;
            }
            steer = closestHit.normal;
            steer = steer.normalized * mine.activeSettings.maxForce;
        }
    }
}