using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace CloudFine.FlockBox.DOTS
{

    public class ColliderAvoidanceSystem : SteeringBehaviorSystem<ColliderAvoidanceData>
    {
        private float3[] Directions;

        protected override void OnCreate()
        {
            base.OnCreate();

            Vector3[] vector3Directions = ColliderAvoidanceBehavior.Directions;
            Directions = new float3[vector3Directions.Length];
            for(int i=0; i<vector3Directions.Length; i++)
            {
                Directions[i] = (float3)vector3Directions[i];
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override JobHandle DoPerception()
        {
            return Dependency;
        }

        protected override JobHandle DoSteering()
        {
            PhysicsWorld physicsWorld = World.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;
            NativeArray<float3> dirs = new NativeArray<float3>(Directions, Allocator.TempJob);
            
            //TODO add sphere casting
            Dependency = Entities
                .WithReadOnly(physicsWorld)
                .WithReadOnly(dirs)
                .ForEach((ref AccelerationData acceleration, ref ColliderAvoidanceData avoidance, in AgentData agent, in SteeringData steering, in LocalToWorld ltw, in LocalToParent ltp, in BoundaryData boundary) =>
                {
                    if (!avoidance.Active) return;

                    float lookDist = math.length(agent.Velocity) * avoidance.LookAheadSeconds;
                    float castRadius = agent.Radius + avoidance.Clearance;
                    CollisionFilter filter = new CollisionFilter()
                    {
                        BelongsTo = (uint)avoidance.LayerMask,
                        CollidesWith = (uint)avoidance.LayerMask,
                    };

                    float3 worldPosition = agent.GetWorldPosition(ltw, ltp);
                    float3 worldForward = agent.GetWorldForward(ltw, ltp);
                    
                    RaycastInput input = new RaycastInput()
                    {
                        Start = worldPosition,
                        End = worldPosition + worldForward * lookDist,
                        Filter = filter
                    };

                    Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();


                    //Something was hit, find best avoidance direction
                    if (physicsWorld.CastRay(input, out hit))
                    {
                        float hitDist = math.length(hit.Position - worldPosition);
                        if (math.all(avoidance.LastClearWorldDirection == float3.zero))
                        {
                            avoidance.LastClearWorldDirection = worldForward;
                        }

                        float3 clearWorldDirection = avoidance.LastClearWorldDirection;

                        float3 up = new float3(0, 1, 0);
                        quaternion rot = quaternion.LookRotation(clearWorldDirection, up);

                        for (int i = 0; i < dirs.Length; i+=avoidance.VisionQuality)
                        {
                            float3 dir = math.mul(rot, dirs[i]);
                            dir = boundary.ValidateDirection(dir);
                            input.End = worldPosition + dir * lookDist;

                            if (!physicsWorld.CastRay(input))
                            {
                                clearWorldDirection = dir;
                                break;                               
                            }
                        }

                        avoidance.LastClearWorldDirection = clearWorldDirection;
                        float smooth = lookDist > 0 ? (1f - (hitDist / lookDist)) : 1f;

                        float3 clearFlockDirection = AgentData.WorldToFlockDirection(ltw, ltp, clearWorldDirection);
                        acceleration.Value += steering.GetSteerVector(clearFlockDirection, agent.Velocity) * avoidance.Weight * smooth;                      
                    }

                    else
                    {
                        avoidance.LastClearWorldDirection = worldForward;
                    }

                }
                ).ScheduleParallel(Dependency);
            Dependency.Complete(); //TODO Not sure why this is necessary, but ECS gets unhappy without it. Definitely not helping performance.
            Dependency = dirs.Dispose(Dependency);
            return Dependency;
        }
    }

    public struct ColliderAvoidanceData : IComponentData
    {
        public bool Active;
        public float Weight;
        public float LookAheadSeconds;
        public float Clearance;
        public Int32 LayerMask;
        public int VisionQuality;
        public float3 LastClearWorldDirection;
    }
}