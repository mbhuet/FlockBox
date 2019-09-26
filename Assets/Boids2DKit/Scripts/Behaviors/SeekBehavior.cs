using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SeekBehavior : SteeringBehavior {

    public const string targetIDAttributeName = "seekTargetID";


    public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsInfo surroundings)
    {
        if (!mine.HasAttribute(targetIDAttributeName)) mine.SetAttribute(targetIDAttributeName, -1);
        int chosenTargetID = (int)mine.GetAttribute(targetIDAttributeName);

        List<Agent> allTargets = GetFilteredAgents(surroundings, this);

        //no targets in neighborhood
        if (allTargets.Count ==0)
        {
            if(chosenTargetID != -1)
            {
                DisengagePursuit(mine, chosenTargetID);
            }
            steer = Vector3.zero;
            return;
        }

        Agent closestTarget = ClosestPursuableTarget(allTargets, mine);

        //no pursuable targets nearby
        if (!closestTarget.CanBePursuedBy(mine)) //double checking because TargetWrapped is a non nullable Struct
        {
            if (chosenTargetID != -1)
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

        AttemptCatch(mine, closestTarget);
        Vector3 desired_velocity = (closestTarget.Position - mine.Position).normalized * mine.activeSettings.maxSpeed;
        steer = desired_velocity - mine.Velocity;
        steer = steer.normalized * Mathf.Min(steer.magnitude, mine.activeSettings.maxForce);

        

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

    static void AttemptCatch(SteeringAgent mine, Agent chosenTargetWrapped)
    {
        float distAway = Vector3.Distance(chosenTargetWrapped.Position, mine.Position);
        if (distAway <= chosenTargetWrapped.Radius && chosenTargetWrapped.CanBePursuedBy(mine))
        {
            mine.CatchAgent(chosenTargetWrapped);
        }
    }


    private static Agent ClosestPursuableTarget(List<Agent> nearbyTargets, Agent agent)
    {
        // int chosenTargetID = (int)agent.GetAttribute(targetIDAttributeName);
        if (nearbyTargets.Count == 0) return null;
        float closeDist = float.MaxValue;
        Agent closeTarget = nearbyTargets[0];
        foreach(Agent target in nearbyTargets)
        {
            float sqrDist = Vector3.SqrMagnitude(target.Position - agent.Position);
            //if(dist <= target.target.radius) AttemptCatch(agent, target);
            if (sqrDist < closeDist && (target.CanBePursuedBy(agent))){
                closeDist = sqrDist;
                closeTarget = target;
            }
        }
        return closeTarget;
    }

}
