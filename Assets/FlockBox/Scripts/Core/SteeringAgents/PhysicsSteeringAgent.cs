using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsSteeringAgent : SteeringAgent
    {
        private new Rigidbody rigidbody;

        protected override void Awake()
        {
            base.Awake();
            rigidbody = GetComponent<Rigidbody>();
        }

        protected override void ApplySteeringAcceleration()
        {

        }


        protected virtual void FixedUpdate()
        {
            Velocity = rigidbody.velocity;
            Velocity += (Acceleration) * Time.fixedDeltaTime;
            Velocity = Velocity.normalized * Mathf.Min(Velocity.magnitude, activeSettings.maxSpeed * speedThrottle);
            ValidateVelocity();
            rigidbody.velocity = Velocity;
            if (Velocity.magnitude > 0)
            {
                rigidbody.MoveRotation(Quaternion.LookRotation(Velocity, Vector3.up));
            }

            Position = transform.localPosition;
            if (!ValidatePosition())
            {
                transform.localPosition = Position;
            }

            
        }
    }
}