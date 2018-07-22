﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vexe.Runtime.Types;

[System.Serializable]
public class PursuitBehavior : SteeringBehavior
{
    public const string targetIDAttributeName = "seekTargetID";


    public override Vector3 GetSteeringBehaviorVector(SteeringAgent mine, SurroundingsInfo surroundings)
    {
        if (!mine.HasAttribute(targetIDAttributeName)) mine.SetAttribute(targetIDAttributeName, -1);
        int chosenTargetID = (int)mine.GetAttribute(targetIDAttributeName);

        //        Debug.Log("pursuit");
        LinkedList<AgentWrapped> allTargets = GetFilteredAgents(surroundings, filterTags);
        
        /*
         * var distance :Vector3D = t.position - position;
            var T :int = distance.length / MAX_VELOCITY;
            futurePosition :Vector3D = t.position + t.velocity * T;
         * 
         */

        //no targets in neighborhood
        if (allTargets.First == null)
        {
            Debug.Log("No Target");
            if (HasPursuitTarget(mine))
            {
                DisengagePursuit(mine, chosenTargetID);
            }
            return Vector3.zero;
        }

        //Debug.Log(allTargets.ToString());

        AgentWrapped closestTarget = ClosestTarget(allTargets, mine);

        //no pursuable targets nearby
        if (!closestTarget.agent.CanBePursuedBy(mine)) //double checking because TargetWrapped is a non nullable Struct
        {
//            Debug.Log("No Pursuable Target");

            if (HasPursuitTarget(mine))
            {
                DisengagePursuit(mine, chosenTargetID);
            }
            return Vector3.zero;
        }


        if (closestTarget.agent.agentID != chosenTargetID)
        {
            DisengagePursuit(mine, chosenTargetID);
            EngagePursuit(mine, closestTarget.agent);
        }

        Vector3 distance = closestTarget.wrappedPosition - mine.position;
        float est_timeToIntercept = distance.magnitude / mine.activeSettings.maxSpeed;
        Vector3 predictedInterceptPosition = closestTarget.wrappedPosition + closestTarget.agent.velocity * est_timeToIntercept;

        AttemptCatch(mine, closestTarget);

        return mine.seek(predictedInterceptPosition) * weight;

    }

    static bool HasPursuitTarget(SteeringAgent mine)
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

    static void AttemptCatch(Agent mine, AgentWrapped chosenQuaryWrapped)
    {
        float distAway = Vector3.Distance(chosenQuaryWrapped.wrappedPosition, mine.position);
        if (distAway <= (chosenQuaryWrapped.agent.radius + mine.radius) && chosenQuaryWrapped.agent.CanBePursuedBy(mine))
        {
            mine.CatchAgent(chosenQuaryWrapped.agent);
//            Debug.Log(chosenQuaryWrapped.agent.name + " successful catch by " + mine.name);
        }
    }


    private static AgentWrapped ClosestTarget(LinkedList<AgentWrapped> nearbyTargets, Agent agent)
    {
        int chosenTargetID = (int)agent.GetAttribute(targetIDAttributeName);

        float closeDist = float.MaxValue;
        AgentWrapped closeTarget = nearbyTargets.First.Value;
        foreach (AgentWrapped target in nearbyTargets)
        {
            Debug.DrawLine(agent.position, target.wrappedPosition, target.agent.CanBePursuedBy(agent)? Color.blue : Color.yellow);
            float dist = Vector3.Distance(target.wrappedPosition, agent.position);
            //if(dist <= target.target.radius) AttemptCatch(agent, target);
            if (dist < closeDist && target.agent.CanBePursuedBy(agent))
            {
                closeDist = dist;
                closeTarget = target;
            }
        }
        return closeTarget;
    }

}
