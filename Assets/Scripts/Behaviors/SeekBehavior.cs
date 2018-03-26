using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vexe.Runtime.Types;

[System.Serializable]
public class SeekBehavior : SteeringBehavior {

    [PerItem, Tags, VisibleWhen("isActive")]
    public string[] targetTags;
    public const string targetIDAttributeName = "targetID";


    public override Vector3 GetSteeringBehaviorVector(SteeringAgent mine, SurroundingsInfo surroundings)
    {
        if (!mine.HasAttribute(targetIDAttributeName)) mine.SetAttribute(targetIDAttributeName, -1);

        int chosenTargetID = (int)mine.GetAttribute(targetIDAttributeName);
        bool chosenTargetFound = false;

        TargetWrapped chosenTargetWrapped = new TargetWrapped();

        LinkedList<TargetWrapped> allTargets = new LinkedList<TargetWrapped>();
        Dictionary<string, LinkedList<TargetWrapped>> sourroundingTargets = surroundings.targets;
        foreach (string tag in targetTags) {
            if (chosenTargetFound) break;

            LinkedList<TargetWrapped> targetsOut;
            if (sourroundingTargets.TryGetValue(tag, out targetsOut)) {
                foreach (TargetWrapped target in targetsOut)
                {
                    allTargets.AddLast(target);
                    if(chosenTargetID != -1 && !chosenTargetFound)
                    {
                        if (target.target.targetID == chosenTargetID)
                        {
                            chosenTargetFound = true;
                            chosenTargetWrapped = target;
                            break;
                        }
                    }
                }
            }
        }

        //
        if (!chosenTargetFound && allTargets.First != null)
        {
            TargetWrapped target = ClosestUnclaimedTarget(allTargets, mine);
            if (target.target.CanBePursuedBy(mine)) //double checking because TargetWrapped is a non nullable Struct
            {
                EngagePursuit(mine, target.target);
                chosenTargetWrapped = target;
                chosenTargetFound = true;
            }
        }

        if (!chosenTargetFound && chosenTargetID!= -1)
        {
            DisengagePursuit(mine, chosenTargetID);
        }

        if (chosenTargetFound) {
            AttemptCatch(mine, chosenTargetWrapped);
            Vector3 desired_velocity = (chosenTargetWrapped.wrappedPosition - mine.position) * mine.settings.maxSpeed;
            Vector3 steering = desired_velocity - mine.velocity;
            return steering;
        }

        

        



            return Vector3.zero;



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


    private TargetWrapped ClosestUnclaimedTarget(LinkedList<TargetWrapped> nearbyTargets, SteeringAgent agent)
    {
        float closeDist = float.MaxValue;
        TargetWrapped closeTarget = nearbyTargets.First.Value;
        foreach(TargetWrapped target in nearbyTargets)
        {
            float dist = Vector3.Distance(target.wrappedPosition, agent.position);
            if(dist <= target.target.radius) AttemptCatch(agent, target);
            if (dist < closeDist && target.target.CanBePursuedBy(agent)){
                closeDist = dist;
                closeTarget = target;
            }
        }
        return closeTarget;
    }

}
