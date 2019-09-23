using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CohesionBehavior : SteeringBehavior {
    // Cohesion
    // For the average position (i.e. center) of all nearby boids, calculate steering vector towards that position
    public override void GetSteeringBehaviorVector(out Vector3 steer, Agent mine, SurroundingsInfo surroundings)
    {
        Vector3 sum = Vector3.zero;   // Start with empty vector to accumulate all positions
        float count = 0;
        foreach (AgentWrapped other in GetFilteredAgents(surroundings, this))
        {
            if (WithinEffectiveRadius(mine, other))
            {
                float modFactor = 1;
                sum += (other.wrappedPosition); // Add position
                count += modFactor; //getting midpoint of weighted positions means dividing total by sum of those weights. Not necessary when getting average of vectors
            }
        }
        if (count > 0)
        {
            sum /= (count);
            mine.GetSeekVector(out steer, sum);  // Steer towards the position
        }
        else
        {
            steer = Vector3.zero;
        }
    }
}
