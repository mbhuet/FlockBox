using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FleeBehavior : SteeringBehavior
{
    public const string fleeAttributeName = "fleeing";

    public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsInfo surroundings)
    {

        Vector3 fleeMidpoint = Vector3.zero;   // Start with empty vector to accumulate all positions
        float count = 0;

        foreach(AgentWrapped other in GetFilteredAgents(surroundings, this))
        {

            if (WithinEffectiveRadius(mine, other))
            {
                

                float modFactor = 1;
                fleeMidpoint += (other.wrappedPosition); // Add position
                count += modFactor; //getting midpoint of weighted positions means dividing total by sum of those weights. Not necessary when getting average of vectors

            }
        }


        if (count > 0)
        {
            fleeMidpoint /= (count);
            Vector3 desired_velocity = (fleeMidpoint - mine.Position).normalized * -1 * mine.activeSettings.maxSpeed;
            steer = desired_velocity - mine.Velocity;
            steer = steer.normalized * Mathf.Min(steer.magnitude, mine.activeSettings.maxForce);

            mine.SetAttribute(fleeAttributeName, true);

        }
        else
        {
            mine.SetAttribute(fleeAttributeName, false);
            steer = Vector3.zero;
        }

    }


    }
