using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Every SteeringAgent uses the same SteeringBehavior instances, there's only one per type and its stored in a static Dictionary
//SteeringBehaviors will never have instance variables
//SteeringAgents have 
namespace CloudFine
{
    [System.Serializable]
    public class SteeringAgent : Agent
    {


        protected float speedThrottle = 1;

        public BehaviorSettings activeSettings;
        private bool freezePosition = false;
        //Takes a type, returns instance

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


        private Vector3 steer = Vector3.zero;

        void Flock(SurroundingsInfo surroundings)
        {
            foreach (SteeringBehavior behavior in activeSettings.Behaviors)
            {
                if (!behavior.IsActive) continue;
                behavior.GetSteeringBehaviorVector(out steer, this, surroundings);
                steer *= behavior.weight;
                if (behavior.drawDebug) Debug.DrawRay(Position, steer, behavior.debugColor);
                ApplyForce(steer);
            }
            if (!myNeighborhood.wrapEdges)
            {
                activeSettings.Containment.GetSteeringBehaviorVector(out steer, this, surroundings);
                ApplyForce(steer);
            }
        }

        public void GetSeekVector(out Vector3 steer, Vector3 target)
        {
            // Steering = Desired minus Velocity
            steer = (target - Position).normalized * activeSettings.maxSpeed - Velocity;
            steer = steer.normalized * Mathf.Min(steer.magnitude, activeSettings.maxForce);
        }

        protected override void UpdateTransform()
        {
            base.UpdateTransform();
            if (Velocity.magnitude > 0)
            {
                Forward = Velocity.normalized;
                transform.localRotation = Quaternion.LookRotation(Forward, Vector3.up);
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