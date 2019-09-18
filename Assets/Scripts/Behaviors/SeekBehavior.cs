using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vexe.Runtime.Types;

[System.Serializable]
public class SeekBehavior : SteeringBehavior {

    public const string targetIDAttributeName = "seekTargetID";


    public override Vector3 GetSteeringBehaviorVector(SteeringAgent mine, SurroundingsInfo surroundings)
    {
        if (!mine.HasAttribute(targetIDAttributeName)) mine.SetAttribute(targetIDAttributeName, -1);
        int chosenTargetID = (int)mine.GetAttribute(targetIDAttributeName);

        LinkedList<AgentWrapped> allTargets = GetFilteredAgents(surroundings, filterTags);

        //no targets in neighborhood
        if (allTargets.First == null)
        {
            if(chosenTargetID != -1)
            {
                DisengagePursuit(mine, chosenTargetID);
            }
            return Vector3.zero;
        }

        AgentWrapped closestTarget = ClosestPursuableTarget(allTargets, mine);

        //no pursuable targets nearby
        if (!closestTarget.agent.CanBePursuedBy(mine)) //double checking because TargetWrapped is a non nullable Struct
        {
            if (chosenTargetID != -1)
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

        AttemptCatch(mine, closestTarget);
        Vector3 desired_velocity = (closestTarget.wrappedPosition - mine.position).normalized * mine.activeSettings.maxSpeed;
        Vector3 steer = desired_velocity - mine.velocity;
        steer = steer.normalized * Mathf.Min(steer.magnitude, mine.activeSettings.maxForce);

        return steer * weight;

    }

    static void EngagePursuit(SteeringAgent mine, Agent target)
    {
        mine.SetAttribute(targetIDAttributeName, target.agentID);
        target.InformOfPursuit(true, mine);
    }

    static void DisengagePursuit(SteeringAgent mine, int targetID)
    {
        mine.SetAttribute(targetIDAttributeName, -1);
        Target.InformOfPursuit(false, mine, targetID);
    }

    static void AttemptCatch(SteeringAgent mine, AgentWrapped chosenTargetWrapped)
    {
        float distAway = Vector3.Distance(chosenTargetWrapped.wrappedPosition, mine.position);
        if (distAway <= chosenTargetWrapped.agent.radius && chosenTargetWrapped.agent.CanBePursuedBy(mine))
        {
            mine.CatchAgent(chosenTargetWrapped.agent);
        }
    }


    private static AgentWrapped ClosestPursuableTarget(LinkedList<AgentWrapped> nearbyTargets, Agent agent)
    {
       // int chosenTargetID = (int)agent.GetAttribute(targetIDAttributeName);

        float closeDist = float.MaxValue;
        AgentWrapped closeTarget = nearbyTargets.First.Value;
        foreach(AgentWrapped target in nearbyTargets)
        {
            float dist = Vector3.Distance(target.wrappedPosition, agent.position);
            //if(dist <= target.target.radius) AttemptCatch(agent, target);
            if (dist < closeDist && (target.agent.CanBePursuedBy(agent))){
                closeDist = dist;
                closeTarget = target;
            }
        }
        return closeTarget;
    }

}
