using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vexe.Runtime.Types;

[System.Serializable]
public class SeekBehavior : SteeringBehavior {

    public const string targetIDAttributeName = "targetID";


    public override Vector3 GetSteeringBehaviorVector(SteeringAgent mine, SurroundingsInfo surroundings)
    {
        if (!mine.HasAttribute(targetIDAttributeName)) mine.SetAttribute(targetIDAttributeName, -1);
        int chosenTargetID = (int)mine.GetAttribute(targetIDAttributeName);

        LinkedList<TargetWrapped> allTargets = new LinkedList<TargetWrapped>();
        Dictionary<string, LinkedList<TargetWrapped>> sourroundingTargets = surroundings.targets;
        foreach (string tag in filterTags) {

            LinkedList<TargetWrapped> targetsOut;
            if (sourroundingTargets.TryGetValue(tag, out targetsOut)) {
                foreach (TargetWrapped target in targetsOut)
                {
                    allTargets.AddLast(target);
                }
            }
        }

        //no targets in neighborhood
        if (allTargets.First == null)
        {
            if(chosenTargetID != -1)
            {
                DisengagePursuit(mine, chosenTargetID);
            }
            return Vector3.zero;
        }

        TargetWrapped closestTarget = ClosestPursuableTarget(allTargets, mine);

        //no pursuable targets nearby
        if (!closestTarget.target.CanBePursuedBy(mine)) //double checking because TargetWrapped is a non nullable Struct
        {
            if (chosenTargetID != -1)
            {
                DisengagePursuit(mine, chosenTargetID);
            }
            return Vector3.zero;
        }


        if (closestTarget.target.targetID != chosenTargetID) 
        {
            DisengagePursuit(mine, chosenTargetID);
            EngagePursuit(mine, closestTarget.target);
        }

        AttemptCatch(mine, closestTarget);
        Vector3 desired_velocity = (closestTarget.wrappedPosition - mine.position).normalized * mine.activeSettings.maxSpeed;
        Vector3 steer = desired_velocity - mine.velocity;
        steer = steer.normalized * Mathf.Min(steer.magnitude, mine.activeSettings.maxForce);

        return steer * weight;

    }

    static void EngagePursuit(SteeringAgent agent, Target target)
    {
        agent.SetAttribute(targetIDAttributeName, target.targetID);
        target.InformOfPursuit(true, agent);
    }

    static void DisengagePursuit(SteeringAgent agent, int targetID)
    {
        agent.SetAttribute(targetIDAttributeName, -1);
        Target.InformOfPursuit(false, agent, targetID);
    }

    static void AttemptCatch(SteeringAgent agent, TargetWrapped chosenTargetWrapped)
    {
        float distAway = Vector3.Distance(chosenTargetWrapped.wrappedPosition, agent.position);
        if (distAway <= chosenTargetWrapped.target.radius && !chosenTargetWrapped.target.isCaught) chosenTargetWrapped.target.CaughtBy(agent);
    }


    private static TargetWrapped ClosestPursuableTarget(LinkedList<TargetWrapped> nearbyTargets, SteeringAgent agent)
    {
        int chosenTargetID = (int)agent.GetAttribute(targetIDAttributeName);

        float closeDist = float.MaxValue;
        TargetWrapped closeTarget = nearbyTargets.First.Value;
        foreach(TargetWrapped target in nearbyTargets)
        {
            float dist = Vector3.Distance(target.wrappedPosition, agent.position);
            //if(dist <= target.target.radius) AttemptCatch(agent, target);
            if (dist < closeDist && (target.target.CanBePursuedBy(agent))){
                closeDist = dist;
                closeTarget = target;
            }
        }
        return closeTarget;
    }

}
