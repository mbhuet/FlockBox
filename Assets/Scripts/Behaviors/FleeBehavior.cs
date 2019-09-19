using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FleeBehavior : SteeringBehavior
{
    public const string fleeAttributeName = "fleeing";

    public override Vector3 GetSteeringBehaviorVector(SteeringAgent mine, SurroundingsInfo surroundings)
    {
        Dictionary<string, LinkedList<AgentWrapped>> sourroundingTargets = surroundings.sortedAgents;

        Vector3 fleeMidpoint = Vector3.zero;   // Start with empty vector to accumulate all positions
        float count = 0;

        foreach(AgentWrapped other in GetFilteredAgents(surroundings, filterTags))
        {
            float d = Vector3.Distance(mine.position, other.wrappedPosition);

            if ((d > 0) && (d < effectiveRadius))
            {
                

                float modFactor = 1;
                fleeMidpoint += (other.wrappedPosition); // Add position
                count += modFactor; //getting midpoint of weighted positions means dividing total by sum of those weights. Not necessary when getting average of vectors

            }
        }


        if (count > 0)
        {
            fleeMidpoint /= (count);
            Vector3 desired_velocity = (fleeMidpoint - mine.position).normalized * -1 * mine.activeSettings.maxSpeed;
            Vector3 steer = desired_velocity - mine.velocity;
            steer = steer.normalized * Mathf.Min(steer.magnitude, mine.activeSettings.maxForce);

            mine.SetAttribute(fleeAttributeName, true);

            return steer * weight;
        }
        else
        {
            mine.SetAttribute(fleeAttributeName, false);
            return new Vector3(0, 0);
        }

    }


    }
