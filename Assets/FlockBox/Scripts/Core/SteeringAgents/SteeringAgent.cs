using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;


namespace CloudFine
{
    [System.Serializable]
    public class SteeringAgent : Agent, IConvertGameObjectToEntity
    {

        public Vector3 Acceleration { get; private set; }

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
                activeSettings.AddPerceptions(this, mySurroundings);
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
                if (behavior.DrawSteering) Debug.DrawRay(transform.position, myNeighborhood.transform.TransformDirection(steerCached), behavior.debugColor);
                ApplyForce(steerCached);
            }
        }

        void Contain()
        {
            if (!myNeighborhood.wrapEdges)
            {
                activeSettings.Containment.GetSteeringBehaviorVector(out steerCached, this, myNeighborhood.WorldDimensions, myNeighborhood.boundaryBuffer);
                if (activeSettings.Containment.DrawSteering) Debug.DrawRay(transform.position, myNeighborhood.transform.TransformDirection(steerCached), activeSettings.Containment.debugColor);
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


       
        protected Quaternion LookRotation(Vector3 desiredForward)
        {
            return Quaternion.LookRotation(desiredForward);
        }

        protected override void UpdateTransform()
        {
            transform.localPosition = (Position);

            if (Velocity.magnitude > 0)
            {
                transform.localRotation = LookRotation(Velocity.normalized);
                Forward = Velocity;
            }

            else
            {
                Forward = transform.localRotation * Vector3.forward;
            }
        }

        void IConvertGameObjectToEntity.Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            //AgentData holds everything a behavior needs to know how to react to another Agent
            dstManager.AddComponentData(entity, new AgentData { 
                Position = Position,
                Velocity = Velocity,
                Forward = Forward,
                Tag = TagMaskUtility.TagToInt(tag),
                Radius = shape.radius
            });

            //give entity a buffer to hold info about surroundings
            dstManager.AddBuffer<SurroundingsData>(entity);

            foreach (SteeringBehavior behavior in activeSettings.Behaviors)
            {
                if(behavior is IConvertToComponentData)
                {
                    (behavior as IConvertToComponentData).Convert(entity, dstManager, conversionSystem);
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = this.transform.localToWorldMatrix;

            if (debugDrawShape)
            {
                Gizmos.color = Color.grey;
                UnityEditor.Handles.matrix = this.transform.localToWorldMatrix;
                UnityEditor.Handles.color = Color.grey;
                shape.DrawGizmo();


            }

           

            if (UnityEditor.Selection.activeGameObject != transform.gameObject)
            {
                return;
            }

            
        }

        void OnDrawGizmos()
        {
            if (activeSettings)
            {
                activeSettings.DrawPropertyGizmos(this);
            }
        }
#endif

    }
}