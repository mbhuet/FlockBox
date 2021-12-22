#if FLOCKBOX_DOTS
using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using CloudFine.FlockBox.DOTS;
using UnityEngine;
using Unity.Transforms;

namespace CloudFine.FlockBox.DOTS
{
    public class LeaderFollowSystem : SteeringBehaviorSystem<LeaderFollowData>
    {
        protected override JobHandle DoPerception()
        {
            return Entities
                .ForEach((ref PerceptionData perception, in LeaderFollowData seek) =>
                {
                    perception.AddGlobalSearchTagMask(seek.TagMask);
                }
                ).ScheduleParallel(Dependency);
        }

        protected override JobHandle DoSteering()
        {
            return Entities
                .ForEach((DynamicBuffer<NeighborData> neighbors, ref AccelerationData acceleration, in LeaderFollowData seek, in AgentData agent, in SteeringData steering
#if UNITY_EDITOR
                , in LocalToWorld ltw, in FlockMatrixData wtf
#endif
                ) =>
                {
                    float3 steer = seek.CalculateSteering(agent, steering, neighbors);
#if UNITY_EDITOR
                    if (seek.DebugSteering) Debug.DrawRay(ltw.Position, wtf.FlockToWorldDirection(steer), seek.DebugColor, 0, true);
#endif
                    acceleration.Value += steer;
                }
                ).ScheduleParallel(Dependency);
        }
    }

    public struct LeaderFollowData : IComponentData
    {
        public bool Active;
        public float Weight;
        public float FollowDistance;
        public float StoppingRadius;
        public float ClearAheadDistance;
        public float ClearAheadRadius;
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
            float closeSqrDist = float.MaxValue;
            bool foundTarget = false;
            float3 closeTargetPosition = float3.zero;
            float3 closeTargetForward = float3.zero;

            for (int i =0; i<neighbors.Length; i++)
            {
                AgentData other = neighbors[i];

                if (other.TagInMask(TagMask))
                {
                    if (!mine.Equals(other))
                    {
                        float sqrDist = math.lengthsq(other.Position - mine.Position);
                        if (sqrDist < closeSqrDist)
                        {
                            closeSqrDist = sqrDist;
                            closeTargetPosition = other.Position;
                            closeTargetForward = other.Forward;
                            foundTarget = true;
                        }
                    }
                }
            }

            if (!foundTarget)
            {
                return float3.zero;
            }

            //check to see if we should clear the way in front of the leader
            float scalar = Vector3.Dot(mine.Position - closeTargetPosition, closeTargetForward);
            if (scalar > 0 && scalar < ClearAheadDistance)//we are somewhere in front of the leader, potentially in the clear zone ahead of it.
            {
                float3 pointOnLeaderPath = closeTargetPosition + closeTargetForward * scalar;
                float insideClearZone = math.lengthsq(pointOnLeaderPath - mine.Position) / (ClearAheadRadius * ClearAheadRadius); //0-1 is inside zone, <1 is outside
                if (insideClearZone <= 1)
                {
                    return float3.zero;
                }
            }

            float3 steer = steering.DesiredVelocityForArrival(mine.Position, closeTargetPosition - closeTargetForward * FollowDistance, StoppingRadius, steering.MaxSpeed) - mine.Velocity;
            return math.normalize(steer) * Mathf.Min(math.length(steer), steering.MaxForce);
        }
    }
}


namespace CloudFine.FlockBox
{
    [DOTSCompatible]
    public partial class LeaderFollowBehavior : IConvertToSteeringBehaviorComponentData<LeaderFollowData>
    {
        
        public LeaderFollowData Convert()
        {
            return new LeaderFollowData
            {
                Active = IsActive,
                Weight = weight,
                FollowDistance = followDistance,
                StoppingRadius = stoppingRadius,
                ClearAheadDistance = clearAheadDistance,
                ClearAheadRadius = clearAheadRadius,
                TagMask = (useTagFilter ? TagMaskUtility.GetTagMask(filterTags) : int.MaxValue),
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