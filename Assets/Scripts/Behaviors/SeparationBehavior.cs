﻿using System.Collections;
using System.Collections.Generic;
using Vexe.Runtime.Types;
using UnityEngine;

[System.Serializable]
public class SeparationBehavior : SteeringBehavior {

    public override Vector3 GetSteeringBehaviorVector(SteeringAgent mine, SurroundingsInfo surroundings)
    {
        Vector3 steer = Vector3.zero;
        int count = 0;
        // For every boid in the system, check if it's too close
        foreach (AgentWrapped other in GetFilteredAgents( surroundings))
        {

            float d = Vector3.Distance(mine.position, other.wrappedPosition);
            // If the distance is greater than 0 and less than an arbitrary amount (0 when you are yourself)
            if ((d > 0) && (d < effectiveRadius))
            {
                // Calculate vector pointing away from neighbor
                Vector3 diff = mine.position - other.wrappedPosition;
                diff.Normalize();
                diff /= (d);        // Weight by distance
                steer += (diff);
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
            steer -= (mine.velocity);
            steer = steer.normalized * Mathf.Min(steer.magnitude, mine.activeSettings.maxForce);
        }
        return steer * weight;
    }
}
