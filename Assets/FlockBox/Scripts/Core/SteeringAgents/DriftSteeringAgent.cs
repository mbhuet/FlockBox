using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    /// <summary>
    /// Will point the direction of Acceleration, not Velocity
    /// May be useful for rockets, starships
    /// </summary>
    public class DriftSteeringAgent : SteeringAgent
    {
        protected override void UpdateTransform()
        {
            transform.localPosition = SmoothedPosition(Position);

            if (Velocity.magnitude > 0)
            {
                transform.localRotation = SmoothedRotation(Acceleration.normalized);
                Forward = Velocity;
            }

            else
            {
                Forward = transform.localRotation * Vector3.forward;
            }
        }

    }
}