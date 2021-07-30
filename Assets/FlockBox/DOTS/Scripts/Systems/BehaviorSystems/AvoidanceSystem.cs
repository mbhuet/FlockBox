#if FLOCKBOX_DOTS
using CloudFine.FlockBox.DOTS;
using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace CloudFine.FlockBox.DOTS
{
    public class AvoidanceSystem : SteeringBehaviorSystem<AvoidanceData>
    {
        protected override JobHandle DoPerception()
        {
            return Entities
                .ForEach((ref PerceptionData perception, in AvoidanceData avoidance) =>
                {
                    perception.ExpandLookAheadSeconds(avoidance.LookAheadSeconds);
                }
                ).ScheduleParallel(Dependency);
        }

        protected override JobHandle DoSteering()
        {
            return Entities
                .ForEach((DynamicBuffer<NeighborData> neighbors, ref AccelerationData acceleration, in AgentData agent, in SteeringData steering, in AvoidanceData avoidance
#if UNITY_EDITOR
                , in LocalToWorld ltw, in FlockMatrixData wtf
#endif
                ) =>
                {
                    float3 steer = avoidance.CalculateSteering(agent, steering, neighbors);
#if UNITY_EDITOR
                    if (avoidance.DebugSteering) Debug.DrawRay(ltw.Position, wtf.FlockToWorldDirection(steer), avoidance.DebugColor, 0, true);
#endif
                    acceleration.Value += steer;
                }
                ).ScheduleParallel(Dependency);
        }
    }

    public struct AvoidanceData : IComponentData
    {
        public bool Active;
        public float Weight;
        public float LookAheadSeconds;
        public float Clearance;
        public Int32 TagMask;
#if UNITY_EDITOR
        public bool DebugSteering;
        public bool DebugProperties;
        public Color32 DebugColor;
#endif

        //TODO keep track for current target for later retrieval
        public float3 CalculateSteering(AgentData mine, SteeringData steering, DynamicBuffer<NeighborData> neighbors)
        {
            if (!Active) return float3.zero;

            if (neighbors.Length == 0)
            {
                return float3.zero;
            }

            float rayDist = LookAheadSeconds * math.length(mine.Velocity);
            float closestHitDist = rayDist;
            float hitDist = 0;
            float3 hitCenter = float3.zero;

            for (int i = 0; i < neighbors.Length; i++)
            {
                AgentData other = neighbors[i].Value;
                if (other.TagInMask(TagMask))
                {
                    if (other.FindRayIntersection(mine.Position, mine.Forward, rayDist, mine.Radius + Clearance, ref hitDist))
                    {
                        if (hitDist < closestHitDist)
                        {
                            closestHitDist = hitDist;
                            hitCenter = other.Position;
                        }
                    }
                }
            }

            if (closestHitDist >= rayDist)
            {
                return float3.zero;
            }

            //inside obstacle, steer out
            if(closestHitDist == 0)
            {
                return math.normalize(mine.Position - hitCenter) * steering.MaxForce * Weight;
            }

            //use projection to find steering direction perpendicular to current forward and away from obstacle center
            return math.normalize(mine.Position + mine.Forward * math.dot(hitCenter - mine.Position, mine.Forward) - hitCenter) 
                * steering.MaxForce 
                * Weight 
                * (1f - (closestHitDist / rayDist));
        }
    }
}

namespace CloudFine.FlockBox
{
    [DOTSCompatible]
    public partial class AvoidanceBehavior : IConvertToSteeringBehaviorComponentData<AvoidanceData>
    {

        public AvoidanceData Convert()
        {
            return new AvoidanceData
            {
                Active = IsActive,
                Weight = weight,
                LookAheadSeconds = lookAheadSeconds,
                TagMask = (useTagFilter ? TagMaskUtility.GetTagMask(filterTags) : int.MaxValue),
                Clearance = clearance,
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