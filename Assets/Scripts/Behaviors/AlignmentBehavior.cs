using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignmentBehavior : SteeringBehavior {
    public new string[] requiredAttributes = {"status"};

    // Alignment
    // For every nearby boid in the system, calculate the average velocity
    public override Vector3 GetSteeringBehaviorVector(SteeringAgent mine, SurroundingsInfo surroundings, float effectiveDistance)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;
        foreach (SteeringAgent other in surroundings.neighbors)
        {

            float d = Vector3.Distance(mine.position, other.position);
            if ((d > 0) && (d < effectiveDistance))
            {
                float modFactor = 1;
                sum += (other.velocity) * modFactor;
                count++;
            }
        }
        if (count > 0)
        {
            sum /= ((float)count);
            // First two lines of code below could be condensed with new Vector3 setMag() method
            // Not using this method until Processing.js catches up
            // sum.setMag(maxspeed);

            // Implement Reynolds: Steering = Desired - Velocity
            sum.Normalize();
            sum *= (mine.maxSpeed);
            Vector3 steer = sum - mine.velocity;
            steer = steer.normalized * Mathf.Min(steer.magnitude, mine.maxforce);
            return steer;
        }
        else
        {
            return new Vector3(0, 0);
        }
    }
}
