using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PursuitBehavior : SteeringBehavior
{
    public const string targetIDAttributeName = "seekTargetID";


    public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsInfo surroundings)
    {
        if (!mine.HasAttribute(targetIDAttributeName)) mine.SetAttribute(targetIDAttributeName, -1);
        int chosenTargetID = (int)mine.GetAttribute(targetIDAttributeName);

        //        Debug.Log("pursuit");
        List<Agent> allTargets = GetFilteredAgents(surroundings, this);
        
        /*
         * var distance :Vector3D = t.position - position;
            var T :int = distance.length / MAX_VELOCITY;
            futurePosition :Vector3D = t.position + t.velocity * T;
         * 
         */

        //no targets in neighborhood
        if (allTargets.Count==0)
        {
            if (HasPursuitTarget(mine))
            {
                DisengagePursuit(mine, chosenTargetID);
            }
            steer = Vector3.zero;
            return;
        }

        //Debug.Log(allTargets.ToString());

        Agent closestTarget = ClosestTarget(allTargets, mine);

        //no pursuable targets nearby
        if (!closestTarget.CanBePursuedBy(mine)) //double checking because TargetWrapped is a non nullable Struct
        {
//            Debug.Log("No Pursuable Target");

            if (HasPursuitTarget(mine))
            {
                DisengagePursuit(mine, chosenTargetID);
            }
            steer = Vector3.zero;
            return;
        }


        if (closestTarget.agentID != chosenTargetID)
        {
            DisengagePursuit(mine, chosenTargetID);
            EngagePursuit(mine, closestTarget);
        }

        Vector3 distance = closestTarget.Position - mine.Position;
        float est_timeToIntercept = distance.magnitude / mine.activeSettings.maxSpeed;
        Vector3 predictedInterceptPosition = closestTarget.Position + closestTarget.Velocity * est_timeToIntercept;

        AttemptCatch(mine, closestTarget);

        mine.GetSeekVector(out steer, predictedInterceptPosition);

    }

    public static bool HasPursuitTarget(SteeringAgent mine)
    {
        if (!mine.HasAttribute(targetIDAttributeName)) return false;
        return (int)mine.GetAttribute(targetIDAttributeName) >= 0;
    
    }

    static void EngagePursuit(SteeringAgent mine, Agent target)
    {
        mine.SetAttribute(targetIDAttributeName, target.agentID);
        target.InformOfPursuit(true, mine);
    }

    static void DisengagePursuit(SteeringAgent mine, int targetID)
    {
        mine.SetAttribute(targetIDAttributeName, -1);
        Agent.InformOfPursuit(false, mine, targetID);

    }

    static void AttemptCatch(Agent mine, Agent chosenQuaryWrapped)
    {
        float distAway = Vector3.Distance(chosenQuaryWrapped.Position, mine.Position);
        if (distAway <= (chosenQuaryWrapped.Radius + mine.Radius) && chosenQuaryWrapped.CanBePursuedBy(mine))
        {
            mine.CatchAgent(chosenQuaryWrapped);
//            Debug.Log(chosenQuaryWrapped.agent.name + " successful catch by " + mine.name);
        }
    }


    private static Agent ClosestTarget(List<Agent> nearbyTargets, Agent agent)
    {
        if (nearbyTargets.Count == 0) return null;

        int chosenTargetID = (int)agent.GetAttribute(targetIDAttributeName);

        float closeDist = float.MaxValue;
        Agent closeTarget = nearbyTargets[0];
        foreach (Agent target in nearbyTargets)
        {
            //Debug.DrawLine(agent.position, target.wrappedPosition, target.agent.CanBePursuedBy(agent)? Color.blue : Color.yellow);
            float dist = (target.Position - agent.Position).sqrMagnitude;
            //if(dist <= target.target.radius) AttemptCatch(agent, target);
            if (dist < closeDist && target.CanBePursuedBy(agent))
            {
                closeDist = dist;
                closeTarget = target;
            }
        }
        return closeTarget;
    }

}
