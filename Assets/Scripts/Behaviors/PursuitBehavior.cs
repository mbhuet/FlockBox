using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vexe.Runtime.Types;

[System.Serializable]
public class PursuitBehavior : SteeringBehavior
{
    public const string pursuitAttributName = "pursuing";


    public override Vector3 GetSteeringBehaviorVector(SteeringAgent mine, SurroundingsInfo surroundings)
    {
        

        LinkedList<AgentWrapped> allTargets = GetFilteredAgents(surroundings, filterTags);

        //no targets in neighborhood
        if (allTargets.First == null)
        {
            mine.SetAttribute(pursuitAttributName, false);

            return Vector3.zero;
        }

        /*
         * var distance :Vector3D = t.position - position;
            var T :int = distance.length / MAX_VELOCITY;
            futurePosition :Vector3D = t.position + t.velocity * T;
         * 
         */

        AgentWrapped closestTarget = ClosestTarget(allTargets, mine);

        Vector3 distance = closestTarget.wrappedPosition - mine.position;
        float est_timeToIntercept = distance.magnitude / mine.activeSettings.maxSpeed;
        Vector3 predictedInterceptPosition = closestTarget.wrappedPosition + closestTarget.agent.velocity * est_timeToIntercept;

        AttemptCatch(mine, closestTarget);
        mine.SetAttribute(pursuitAttributName, true);

        return mine.seek(predictedInterceptPosition) * weight;

    }

    
    static void AttemptCatch(Agent mine, AgentWrapped chosenQuaryWrapped)
    {
        float distAway = Vector3.Distance(chosenQuaryWrapped.wrappedPosition, mine.position);
        if (distAway <= (chosenQuaryWrapped.agent.radius + mine.radius))
        {
            mine.CatchAgent(chosenQuaryWrapped.agent);
            //Debug.Log(chosenQuaryWrapped.agent.name + " successful catch by " + mine.name);
        }
    }


    private static AgentWrapped ClosestTarget(LinkedList<AgentWrapped> nearbyTargets, Agent agent)
    {
        
        float closeDist = float.MaxValue;
        AgentWrapped closeTarget = nearbyTargets.First.Value;
        foreach (AgentWrapped target in nearbyTargets)
        {
            float dist = Vector3.Distance(target.wrappedPosition, agent.position);
            //if(dist <= target.target.radius) AttemptCatch(agent, target);
            if (dist < closeDist)
            {
                closeDist = dist;
                closeTarget = target;
            }
        }
        return closeTarget;
    }

}
