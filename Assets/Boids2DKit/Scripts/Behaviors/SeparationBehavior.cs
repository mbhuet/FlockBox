using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SeparationBehavior : SteeringBehavior {

    public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsInfo surroundings)
    {
        steer = Vector3.zero;
        int count = 0;
        // For every boid in the system, check if it's too close
        foreach (Agent other in GetFilteredAgents( surroundings, this))
        {

            if(WithinEffectiveRadius(mine, other))
            {
                // Calculate vector pointing away from neighbor
                Vector3 diff = mine.Position - other.Position;
                if (diff.sqrMagnitude<.001f) diff = UnityEngine.Random.insideUnitCircle * .01f;
                //weighted by distance
                steer += (diff.normalized/diff.magnitude);
                count++;            // Keep track of how many
            }
        }
        // Average -- divide by how many
        if (count > 0)
        {
            steer /= ((float)count);
        }

        // As long as the vector is greater than 0
        if (steer.magnitude > 0)
        {
            // First two lines of code below could be condensed with new Vector3 setMag() method
            // Not using this method until Processing.js catches up
            // steer.setMag(maxspeed);

            // Implement Reynolds: Steering = Desired - Velocity
            steer = steer.normalized * (mine.activeSettings.maxSpeed);
            steer -= (mine.Velocity);
            steer = steer.normalized * Mathf.Min(steer.magnitude, mine.activeSettings.maxForce);
        }
    }
}
