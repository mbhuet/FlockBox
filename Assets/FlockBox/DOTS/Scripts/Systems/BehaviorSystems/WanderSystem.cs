#if FLOCKBOX_DOTS
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using CloudFine.FlockBox.DOTS;
using UnityEngine;
using Unity.Transforms;

namespace CloudFine.FlockBox.DOTS
{
    public class WanderSystem : SteeringBehaviorSystem<WanderData>
    {

        protected override JobHandle DoPerception()
        {
            return Dependency;
        }

        protected override JobHandle DoSteering()
        {
            double time = Time.ElapsedTime;
            return Entities
                .ForEach((ref AccelerationData acceleration, in AgentData agent, in WanderData wander, in SteeringData steering, in BoundaryData boundary
#if UNITY_EDITOR
                , in LocalToWorld ltw, in FlockMatrixData wtf
#endif
                ) =>
                {
                    var steer = wander.CalculateSteering(agent, steering, (float)time, boundary);
#if UNITY_EDITOR
                    if (wander.DebugSteering) Debug.DrawRay(ltw.Position, wtf.FlockToWorldDirection(steer), wander.DebugColor, 0, true);
#endif
                    acceleration.Value += steer;
                }
                ).ScheduleParallel(Dependency);
        }
    }

    public struct WanderData : IComponentData
    {
        public bool Active;
        public float Weight;
        public float Scope;
        public float Intensity;
#if UNITY_EDITOR
        public bool DebugSteering;
        public bool DebugProperties;
        public Color32 DebugColor;
#endif

        public float3 CalculateSteering(AgentData mine, SteeringData steering, float time, BoundaryData boundary)
        {
            if (!Active) return float3.zero;

            float UniqueID = mine.UniqueID *.001f;
            UniqueID *= UniqueID;

            float3 dir = new float3(
                (noise.cnoise(new float2((time * Intensity), UniqueID))) * Scope * .5f,
                (noise.cnoise(new float2((time * Intensity) + UniqueID, UniqueID))) * Scope * .5f,
                (noise.cnoise(new float2((time * Intensity) + UniqueID * 2, UniqueID))) * Scope * .5f
                );

            return boundary.ValidateDirection(math.mul(quaternion.Euler(math.radians(dir)), mine.Forward))
                    * steering.MaxForce
                    * Weight;
        }
    }
}

namespace CloudFine.FlockBox
{
    [DOTSCompatible]
    public partial class WanderBehavior: IConvertToSteeringBehaviorComponentData<WanderData>
    {
        public WanderData Convert()
        {
            return new WanderData
            {
                Active = IsActive,
                Weight = weight,
                Intensity = wanderIntensity,
                Scope = wanderScope,
#if UNITY_EDITOR
                DebugSteering = DrawSteering,
                DebugProperties = DrawProperties,
                DebugColor = debugColor
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