using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine.FlockBox
{
    /// <summary>
    /// Will point the direction of Acceleration, not Velocity
    /// May be useful for rockets, starships
    /// </summary>
    public class DriftSteeringAgent : SteeringAgent
    {
        protected override void UpdateTransform()
        {
            transform.position = FlockBoxToWorldPosition(Position);

            if (Velocity.magnitude > 0)
            {
                transform.rotation = LookRotation(FlockBoxToWorldDirection(Acceleration).normalized);
                Forward = Velocity;
            }

            else
            {
                Forward = WorldToFlockBoxDirection(transform.rotation * Vector3.forward);
            }
        }

    }
}