using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vexe.Runtime.Types;


[System.Serializable]
public class AvoidanceBehavior : SteeringBehavior {

    [PerItem, Tags, VisibleWhen("isActive")]
    public string[] avoidTags;

    public override Vector3 GetSteeringBehaviorVector(SteeringAgent mine, SurroundingsInfo surroundings)
    {
        LinkedList<ZoneWrapped> obstacles = GetAvoidZones(surroundings);
        if (obstacles.First == null) return Vector3.zero;
        bool foundObstacleInPath = false;
        float closestHitDistance = float.MaxValue;
        Vector3 closestHitPoint = Vector3.zero;
        Zone mostThreateningObstacle = obstacles.First.Value.zone;

        foreach (ZoneWrapped obs_wrapped in obstacles)
        {
            Zone zone = obs_wrapped.zone;
            Vector3 closestPoint = ClosestPointPathToObstacle(mine, obs_wrapped);
            if (Vector3.Distance(closestPoint, obs_wrapped.wrappedCenter) < zone.radius)
            {
                //found obstacle directly in path
                foundObstacleInPath = true;

                float distanceToClosestPoint = Vector3.Distance(closestPoint, mine.position);
                if (distanceToClosestPoint < closestHitDistance)
                {
                    closestHitDistance = distanceToClosestPoint;
                    closestHitPoint = closestPoint;
                    mostThreateningObstacle = zone;
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

    LinkedList<ZoneWrapped> GetAvoidZones(SurroundingsInfo surroundings)
    {
        Dictionary<string, LinkedList<ZoneWrapped>> zoneDict = surroundings.zones;
        LinkedList<ZoneWrapped> obstacles = new LinkedList<ZoneWrapped>();

        LinkedList<ZoneWrapped> zonesOut = new LinkedList<ZoneWrapped>();
        foreach(string avoidTag in avoidTags)
        {
            if(zoneDict.TryGetValue(avoidTag, out zonesOut))
            {
                foreach (ZoneWrapped avoidZone in zonesOut)
                {
                    obstacles.AddLast(avoidZone);
                }

            }
        }
        return obstacles;
    }

    Vector3 ClosestPointPathToObstacle(SteeringAgent agent, ZoneWrapped obstacle)
    {
        Vector3 agentPos = agent.position;
        Vector3 agentToObstacle = obstacle.wrappedCenter - agentPos;
        Vector3 projection = Vector3.Project(agentToObstacle, agent.velocity.normalized);
        if (projection.normalized == agent.velocity.normalized)
            return agentPos + projection;
        else return agentPos;
    }
}

