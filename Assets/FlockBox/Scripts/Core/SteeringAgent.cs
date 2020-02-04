using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CloudFine
{
    [System.Serializable]
    public class SteeringAgent : Agent
    {

        private Vector3 Acceleration = Vector3.zero;
        protected float speedThrottle = 1;

        public BehaviorSettings activeSettings;
        private bool freezePosition = false;
        private bool sleeping = false;
        private SurroundingsContainer mySurroundings = new SurroundingsContainer();
        protected virtual void Update()
        {
            if (!isAlive) return;
            if (activeSettings == null) return;
            if (freezePosition) return;
            sleeping = (UnityEngine.Random.value < myNeighborhood.sleepChance);
            if(!sleeping){
                Acceleration *= 0;
                activeSettings.AddPerceptions(mySurroundings);
                myNeighborhood.GetSurroundings(Position, Velocity, buckets, mySurroundings);
                Flock(mySurroundings);
            }
            Contain();
            ApplySteeringAcceleration(Acceleration);
        }


        protected override void LateUpdate()
        {
            if (!isAlive) return;
            if (sleeping) return;
            FindNeighborhoodBuckets();
        }


        void ApplyForce(Vector3 force)
        {
            // We could add mass here if we want A = F / M
            Acceleration += (force);
        }


        private Vector3 steerCached = Vector3.zero;

        void Flock(SurroundingsContainer surroundings)
        {
            foreach (SteeringBehavior behavior in activeSettings.Behaviors)
            {
                if (!behavior.IsActive) continue;
                behavior.GetSteeringBehaviorVector(out steerCached, this, surroundings);
                steerCached *= behavior.weight;
                if (behavior.drawDebug) Debug.DrawRay(transform.position, myNeighborhood.transform.TransformDirection(steerCached), behavior.debugColor);
                ApplyForce(steerCached);
            }
            
        }

        void Contain()
        {
            if (!myNeighborhood.wrapEdges)
            {
                activeSettings.Containment.GetSteeringBehaviorVector(out steerCached, this, myNeighborhood.WorldDimensions, myNeighborhood.boundaryBuffer);
                if (activeSettings.Containment.drawDebug) Debug.DrawRay(transform.position, myNeighborhood.transform.TransformDirection(steerCached), activeSettings.Containment.debugColor);
                ApplyForce(steerCached);
            }
        }

        public void GetSeekVector(out Vector3 steer, Vector3 target)
        {
            steer = (target - Position).normalized * activeSettings.maxSpeed - Velocity;
            steer = steer.normalized * Mathf.Min(steer.magnitude, activeSettings.maxForce);
        }

        /// <summary>
        /// Apply the results of all steering behavior calculations to this object.
        /// </summary>
        /// <param name="acceleration">The result of all steering behaviors.</param>
        protected virtual void ApplySteeringAcceleration(Vector3 acceleration)
        {
            Velocity += (acceleration) * Time.deltaTime;
            Velocity = Velocity.normalized * Mathf.Min(Velocity.magnitude, activeSettings.maxSpeed * speedThrottle);
            ValidateVelocity();

            Position += (Velocity * Time.deltaTime);
            ValidatePosition();

            UpdateTransform();
        }


        protected override void FindNeighborhoodBuckets()
        {
            if (myNeighborhood)
                myNeighborhood.UpdateAgentBuckets(this, buckets, false);
        }


        public override void Spawn(FlockBox neighborhood)
        {
            base.Spawn(neighborhood);
            LockPosition(false);
            speedThrottle = 1;
            Acceleration = Vector3.zero;
            if (activeSettings)
            {
                Velocity = UnityEngine.Random.insideUnitSphere * activeSettings.maxSpeed;
            }
            else
            {
                Debug.LogWarning("No BehaviorSettings for SteeringAgent " + this.name);
            }
        }

        protected void LockPosition(bool isLocked)
        {
            freezePosition = isLocked;
        }
    }
}