using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CohesionBehavior : SteeringBehavior {

    // Cohesion
    // For the average position (i.e. center) of all nearby boids, calculate steering vector towards that position
    public override Vector3 GetSteeringBehaviorVector(SteeringAgent mine, SurroundingsInfo surroundings, float effectiveDistance)
    {
        Vector3 sum = Vector3.zero;   // Start with empty vector to accumulate all positions
        float count = 0;
        foreach (SteeringAgent other in surroundings.neighbors)
        {

            float d = Vector3.Distance(mine.position, other.position);
            if ((d > 0) && (d < effectiveDistance))
            {
                float modFactor = 1;
                sum += (other.position); // Add position
                count += modFactor; //getting midpoint of weighted positions means dividing total by sum of those weights. Not necessary when getting average of vectors
            }
        }
        if (count > 0)
        {
            sum /= (count);
            return mine.seek(sum);  // Steer towards the position
        }
        else
        {
            return new Vector3(0, 0);
        }
    }
}
