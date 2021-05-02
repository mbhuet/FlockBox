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
            Vector3 planetoidCenter = _flockBox.WorldDimensions/2f;
            float planetoidRadius = Mathf.Min(_flockBox.WorldDimensions.x, _flockBox.WorldDimensions.y, _flockBox.WorldDimensions.z)/2f;
            ///////

            Vector3 planetoidUp = (Position - planetoidCenter).normalized;
            Vector3 clampedPosition = planetoidCenter + planetoidUp * planetoidRadius;

            Position = clampedPosition;
            transform.localPosition = Position;


            //project velocity onto plane tangent to planetoid surface
            Velocity = Vector3.ProjectOnPlane(Velocity, planetoidUp);


            if (Velocity.magnitude > 0)
            {
                transform.localRotation = Quaternion.LookRotation(Velocity, planetoidUp);
                Forward = Velocity;
            }

            else
            {
                Forward = transform.localRotation * Vector3.forward;
            }
        }

    }
}