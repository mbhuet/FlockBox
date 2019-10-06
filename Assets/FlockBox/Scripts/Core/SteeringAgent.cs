using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CloudFine
{
    [System.Serializable]
    public class SteeringAgent : Agent
    {
        protected float speedThrottle = 1;

        public BehaviorSettings activeSettings;
        private bool freezePosition = false;

        private SurroundingsInfo mySurroundings = new SurroundingsInfo();
        protected virtual void Update()
        {
            if (!isAlive) return;
            if (activeSettings == null) return;
            if (freezePosition) return;

            activeSettings.AddPerceptions(ref mySurroundings);
            myNeighborhood.GetSurroundings(Position, Velocity, ref buckets, ref mySurroundings);
            Flock(mySurroundings);

            Velocity += (Acceleration) * Time.deltaTime;
            Velocity = Velocity.normalized * Mathf.Min(Velocity.magnitude, activeSettings.maxSpeed * speedThrottle);
            Velocity = new Vector3(mySurroundings.worldDimensions.x > 0 ? Velocity.x : 0,
                mySurroundings.worldDimensions.y > 0 ? Velocity.y : 0,
                mySurroundings.worldDimensions.z > 0 ? Velocity.z : 0);
            Position += (Velocity * Time.deltaTime);
            ValidatePosition();
            Acceleration *= 0;

            UpdateTransform();
        }


        protected override void LateUpdate()
        {
            if (!isAlive) return;
            FindNeighborhoodBuckets();
        }


        void ApplyForce(Vector3 force)
        {
            // We could add mass here if we want A = F / M
            Acceleration += (force);
        }


        private Vector3 steerCached = Vector3.zero;

        void Flock(SurroundingsInfo surroundings)
        {
            foreach (SteeringBehavior behavior in activeSettings.Behaviors)
            {
                if (!behavior.IsActive) continue;
                behavior.GetSteeringBehaviorVector(out steerCached, this, surroundings);
                steerCached *= behavior.weight;
                if (behavior.drawDebug) Debug.DrawRay(Position, steerCached, behavior.debugColor);
                ApplyForce(steerCached);
            }
            if (!myNeighborhood.wrapEdges)
            {
                activeSettings.Containment.GetSteeringBehaviorVector(out steerCached, this, surroundings);
                if (activeSettings.Containment.drawDebug) Debug.DrawRay(Position, steerCached, activeSettings.Containment.debugColor);
                ApplyForce(steerCached);
            }
        }

        public void GetSeekVector(out Vector3 steer, Vector3 target)
        {
            steer = (target - Position).normalized * activeSettings.maxSpeed - Velocity;
            steer = steer.normalized * Mathf.Min(steer.magnitude, activeSettings.maxForce);
        }

        protected override void UpdateTransform()
        {
            
            base.UpdateTransform();
            if (Velocity.magnitude > 0)
            {
                Forward = Velocity.normalized;
                transform.localRotation = Quaternion.LookRotation(Forward, myNeighborhood.Up);
            }
        }


        public override void Spawn(FlockBox neighborhood)
        {
            base.Spawn(neighborhood);
            LockPosition(false);
            speedThrottle = 1;
            Acceleration = Vector3.zero;
            Forward = UnityEngine.Random.insideUnitSphere;
            Velocity = Forward * activeSettings.maxSpeed;
        }

        protected void LockPosition(bool isLocked)
        {
            freezePosition = isLocked;
        }
    }
}