using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CloudFine
{
    [System.Serializable]
    public class SteeringAgent : Agent
    {

        protected Vector3 Acceleration { get; private set; }

        protected float speedThrottle = 1;

        public BehaviorSettings activeSettings;
        private bool freezePosition = false;
        private bool sleeping = false;
        private SurroundingsContainer mySurroundings = new SurroundingsContainer();


        //Dampening
        [SerializeField, HideInInspector] protected bool _smoothRotation;
        [SerializeField, HideInInspector] protected bool _smoothPosition;

        [SerializeField, HideInInspector] private float _rotationTension = .3f;
        [SerializeField, HideInInspector] private float _positionTension = 1;
        [SerializeField, HideInInspector] private float _positionSlackDistance = 0;
        [SerializeField, HideInInspector] private float _rotationSlackDegrees = 10;

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
            ApplySteeringAcceleration();
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
            GetSteerVector(out steer, target - Position);
        }

        public void GetSteerVector(out Vector3 steer, Vector3 desiredForward)
        {
            steer = desiredForward.normalized * activeSettings.maxSpeed - Velocity;
            steer = steer.normalized * Mathf.Min(steer.magnitude, activeSettings.maxForce);
        }

        /// <summary>
        /// Apply the results of all steering behavior calculations to this object.
        /// </summary>
        protected virtual void ApplySteeringAcceleration()
        {
            Velocity += (Acceleration) * Time.deltaTime;
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


        public override void Spawn(FlockBox neighborhood, Vector3 position)
        {
            base.Spawn(neighborhood, position);
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        protected Vector3 SmoothedPosition(Vector3 targetPosition)
        {
            if (!_smoothPosition) return targetPosition;

            float positionSlack = (transform.localPosition - targetPosition).sqrMagnitude / (_positionSlackDistance * _positionSlackDistance);
            positionSlack *= positionSlack;
            positionSlack = Mathf.Clamp01(positionSlack);

            //lerp(target, value, expE(-rate[0-1] * deltaTime))
            return Vector3.Lerp(transform.localPosition, targetPosition, Mathf.Exp(-(_positionTension * positionSlack) * Time.deltaTime));
        }

        protected Quaternion SmoothedRotation(Vector3 targetForward)
        {
            Quaternion desiredLocalRotation = Quaternion.LookRotation(targetForward.normalized, Vector3.up);
            if (!_smoothRotation) return desiredLocalRotation;

            float rotationSlack = Quaternion.Angle(transform.localRotation, desiredLocalRotation) / _rotationSlackDegrees;
            rotationSlack *= rotationSlack;
            rotationSlack = Mathf.Clamp01(rotationSlack);

            //lerp(target, value, expE(-rate[0-1] * deltaTime))
            return Quaternion.Slerp(transform.localRotation, desiredLocalRotation, Mathf.Exp(-(_rotationTension * rotationSlack) * Time.deltaTime));
        }

        protected override void UpdateTransform()
        {
            transform.localPosition = SmoothedPosition(Position);

            if (Velocity.magnitude > 0)
            {
                transform.localRotation = SmoothedRotation(Velocity.normalized);
                Forward = Velocity.normalized;
            }

            else
            {
                Forward = transform.localRotation * Vector3.forward;
            }
        }
    }
}