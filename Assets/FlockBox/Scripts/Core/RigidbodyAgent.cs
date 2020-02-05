using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyAgent : SteeringAgent
    {
        private Rigidbody rigidbody;

        protected override void Awake()
        {
            base.Awake();
            rigidbody = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Apply the results of all steering behavior calculations to this object.
        /// </summary>
        /// <param name="acceleration">The result of all steering behaviors.</param>
        protected override void ApplySteeringAcceleration(Vector3 acceleration)
        {
            Velocity = rigidbody.velocity;
            Velocity += (acceleration) * Time.deltaTime;
            Velocity = Velocity.normalized * Mathf.Min(Velocity.magnitude, activeSettings.maxSpeed * speedThrottle);
            ValidateVelocity();
        }

        protected virtual void FixedUpdate()
        {
            if (rigidbody)
            {
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
}