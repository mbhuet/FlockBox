using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine.FlockBox
{
    public class PlanetoidSteeringAgent : SteeringAgent
    {
        protected override void UpdateTransform()
        {

            ///////
            ///The planetoid's center and radius are assumed to be the center of the FlockBox and its smallest dimension.
            Vector3 planetoidCenter = FlockBox.WorldDimensions/2f;
            float planetoidRadius = Mathf.Min(FlockBox.WorldDimensions.x, FlockBox.WorldDimensions.y, FlockBox.WorldDimensions.z)/2f;
            ///////

            Vector3 planetoidUp = (Position - planetoidCenter).normalized;
            Vector3 clampedPosition = planetoidCenter + planetoidUp * planetoidRadius;

            Position = clampedPosition;
            transform.position = FlockBoxToWorldPosition(Position);


            //project velocity onto plane tangent to planetoid surface
            Velocity = Vector3.ProjectOnPlane(Velocity, planetoidUp);


            if (Velocity.magnitude > 0)
            {
                transform.rotation = Quaternion.LookRotation(FlockBoxToWorldDirection(Velocity), FlockBoxToWorldDirection(planetoidUp));
                Forward = Velocity;
            }

            else
            {
                Forward = WorldToFlockBoxDirection(transform.rotation * Vector3.forward);
            }
        }

    }
}