using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

namespace CloudFine.FlockBox.DOTS
{

    public class ColliderAvoidanceSystem : SteeringBehaviorSystem<ColliderAvoidanceData>
    {

        protected override JobHandle DoPerception()
        {
            return Dependency;
        }

        protected override JobHandle DoSteering()
        {
            PhysicsWorld physicsWorld = World.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;

            return Entities
                .WithoutBurst()
                .WithReadOnly(physicsWorld)
                .ForEach((ref AccelerationData acceleration, in AgentData agent, in SteeringData steering, in ColliderAvoidanceData avoidance) =>
                {
                    float lookDist = math.length(agent.Velocity) * avoidance.LookAheadSeconds;
                    CollisionFilter filter = new CollisionFilter()
                    {
                        CollidesWith = (uint)avoidance.LayerMask
                    };
                    //TODO use LocalToWorld to get world position
                    float3 worldPosition = agent.Position;
                    float3 worldForward = agent.Forward;

                    RaycastInput input = new RaycastInput()
                    {
                        Start = worldPosition,
                        End = worldPosition + worldForward * 10,
                        Filter = CollisionFilter.Default,                    
                    };
                    //Something was hit, find best avoidance direction
                    if (physicsWorld.CastRay(input))
                    //physicsWorld.SphereCast(worldPosition, agent.Radius + avoidance.Clearance, worldForward, lookDist, filter))
                    {
                        Debug.Log("hit");
                        acceleration.Value -= worldForward * 10;
                    }

                    else
                    {
                        //avoidance.LastClearDirection = worldForward;
                    }

                }
                ).ScheduleParallel(Dependency);
        }
    }

    public struct ColliderAvoidanceData : IComponentData
    {
        public bool Active;
        public float Weight;
        public float LookAheadSeconds;
        public float Clearance;
        public Int32 LayerMask;
    }
}