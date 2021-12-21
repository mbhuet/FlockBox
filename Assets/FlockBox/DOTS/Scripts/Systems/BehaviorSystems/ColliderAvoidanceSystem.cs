#if FLOCKBOX_DOTS
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using CloudFine.FlockBox.DOTS;

namespace CloudFine.FlockBox.DOTS
{

    public class ColliderAvoidanceSystem : SteeringBehaviorSystem<ColliderAvoidanceData>
    {
        private float3[] Directions;
        private BuildPhysicsWorld buildPhysicsWorld;

        protected override void OnCreate()
        {
            base.OnCreate();

            Vector3[] vector3Directions = ColliderAvoidanceBehavior.Directions;
            Directions = new float3[vector3Directions.Length];
            for(int i=0; i<vector3Directions.Length; i++)
            {
                Directions[i] = (float3)vector3Directions[i];
            }
            buildPhysicsWorld = World.GetExistingSystem<BuildPhysicsWorld>();
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
            PhysicsWorld physicsWorld = buildPhysicsWorld.PhysicsWorld;
            NativeArray<float3> dirs = new NativeArray<float3>(Directions, Allocator.TempJob);

            Dependency = JobHandle.CombineDependencies(Dependency, buildPhysicsWorld.GetOutputDependency());

            //TODO add sphere casting
            Dependency = Entities
                .WithReadOnly(physicsWorld)
                .WithReadOnly(dirs)
                .ForEach((ref AccelerationData acceleration, ref ColliderAvoidanceData avoidance, in AgentData agent, in SteeringData steering, in LocalToWorld ltw, in FlockMatrixData wtf, in BoundaryData boundary) =>
                {
                    if (!avoidance.Active) return;

                    float lookDist = math.length(agent.Velocity) * avoidance.LookAheadSeconds;
                    float castRadius = agent.Radius + avoidance.Clearance;
                    CollisionFilter filter = new CollisionFilter()
                    {
                        BelongsTo = (uint)avoidance.LayerMask,
                        CollidesWith = (uint)avoidance.LayerMask,
                    };

                    float3 worldPosition = ltw.Position;
                    float3 worldForward = ltw.Forward;
                    
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

                            if (!physicsWorld.CastRay(input, out hit))
                            {
#if UNITY_EDITOR
                                if (avoidance.DebugVision) Debug.DrawLine(input.Start, input.End, Color.yellow);
#endif
                                clearWorldDirection = dir;
                                break;                               
                            }
                            else
                            {
#if UNITY_EDITOR
                                if (avoidance.DebugVision) Debug.DrawLine(input.Start, hit.Position, new Color(1, 1, 1, .3f));
#endif
                            }
                        }

                        avoidance.LastClearWorldDirection = clearWorldDirection;
                        float smooth = lookDist > 0 ? (1f - (hitDist / lookDist)) : 1f;

                        float3 clearFlockDirection = wtf.WorldToFlockDirection(clearWorldDirection);

                        float3 steer = steering.GetSteerVector(clearFlockDirection, agent.Velocity) * avoidance.Weight * smooth;
#if UNITY_EDITOR
                        if (avoidance.DebugSteering)
                        {
                            Debug.DrawRay(ltw.Position, wtf.FlockToWorldDirection(steer), avoidance.DebugColor, 0, true);
                        }
#endif
                        acceleration.Value += steer;
                    }

                    else
                    {
                        avoidance.LastClearWorldDirection = worldForward;
                    }

                }
                ).ScheduleParallel(Dependency);
            buildPhysicsWorld.AddInputDependencyToComplete(Dependency);
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
#if UNITY_EDITOR
        public bool DebugSteering;
        public bool DebugProperties;
        public bool DebugVision;
        public Color32 DebugColor;
#endif
    }
}

namespace CloudFine.FlockBox
{
    [DOTSCompatible]
    public partial class ColliderAvoidanceBehavior : IConvertToSteeringBehaviorComponentData<ColliderAvoidanceData>
    {
        public ColliderAvoidanceData Convert()
        {
            return new ColliderAvoidanceData
            {
                Active = IsActive,
                Weight = weight,
                LookAheadSeconds = lookAheadSeconds,
                LayerMask = mask,
                Clearance = clearance,
                VisionQuality = (int)visionRayDensity,
                LastClearWorldDirection = Unity.Mathematics.float3.zero,
#if UNITY_EDITOR
                DebugColor = debugColor,
                DebugSteering = DrawSteering,
                DebugProperties = DrawProperties,
                DebugVision = drawVisionRays,
#endif
            };
        }

        public bool HasEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.HasEntityData(this, entity, entityManager);
        public void AddEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.AddEntityData(this, entity, entityManager);
        public void SetEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.SetEntityData(this, entity, entityManager);
        public void RemoveEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.RemoveEntityData(this, entity, entityManager);
    }
}
#endif