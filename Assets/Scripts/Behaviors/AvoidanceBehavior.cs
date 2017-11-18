using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidanceBehavior : SteeringBehavior {

    public override Vector3 GetSteeringBehaviorVector(SteeringAgent mine, SurroundingsInfo surroundings, float effectiveDistance)
    {
        Vector3 steer = Vector3.zero;
        int count = 0;
        foreach (Obstacle obstacle in surroundings.obstacles)
        {

            float dist = Vector3.Distance(mine.position, obstacle.center);
            if (dist < obstacle.radius + Obstacle.forceFieldDistance)
            {
                Vector3 away = (mine.position - obstacle.center).normalized;
                float force = Mathf.Clamp01(1 - (dist - obstacle.radius) / Obstacle.forceFieldDistance);
                force = force * force;
                //Debug.Log(force);

                steer += (away * force);
                count++;
            }
        }

        if (count > 0)
        {
            steer /= (count);
        }

        // As long as the vector is greater than 0
        if (steer.magnitude > 0)
        {
            // First two lines of code below could be condensed with new Vector3 setMag() method
            // Not using this method until Processing.js catches up
            // steer.setMag(maxspeed);

            // Implement Reynolds: Steering = Desired - Velocity
            steer = steer.normalized * (mine.maxSpeed);
            steer -= (mine.velocity);
            steer = steer.normalized * Mathf.Min(steer.magnitude, mine.maxforce);
        }
        return steer;
    }
}
