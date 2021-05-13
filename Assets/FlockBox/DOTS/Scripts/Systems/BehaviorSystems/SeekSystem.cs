#if FLOCKBOX_DOTS
using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using CloudFine.FlockBox.DOTS;

namespace CloudFine.FlockBox.DOTS
{
    public class SeekSystem : SteeringBehaviorSystem<SeekData>
    {
        protected override JobHandle DoPerception()
        {
            return Entities
                .ForEach((ref PerceptionData perception, in SeekData seek) =>
                {
                    if (seek.GlobalTagSearch)
                    {
                        perception.AddGlobalSearchTagMask(seek.TagMask);
                    }
                    else
                    {
                        perception.ExpandPerceptionRadius(seek.Radius);
                    }
                }
                ).ScheduleParallel(Dependency);
        }

        protected override JobHandle DoSteering()
        {
            return Entities
                .ForEach((DynamicBuffer<NeighborData> neighbors, ref AccelerationData acceleration, in SeekData seek, in AgentData agent, in SteeringData steering) =>
                {
                    acceleration.Value += seek.CalculateSteering(agent, steering, neighbors);
                }
                ).ScheduleParallel(Dependency);
        }
    }

    public struct SeekData : IComponentData
    {
        public bool Active;
        public float Weight;
        public float Radius;
        public Int32 TagMask;
        public bool GlobalTagSearch;

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

            for (int i =0; i<neighbors.Length; i++)
            {
                AgentData other = neighbors[i];

                    if (other.TagInMask(TagMask))
                    {
                    if (!mine.Equals(other))
                    {
                        float sqrDist = math.lengthsq(other.Position - mine.Position);
                        if (sqrDist < closeSqrDist && sqrDist < Radius * Radius)
                        {
                            closeSqrDist = sqrDist;
                            closeTargetPosition = other.Position;
                            foundTarget = true;
                        }
                    }
                }
            }

            if (!foundTarget)
            {
                return float3.zero;
            }

            return steering.GetSteerVector((closeTargetPosition - mine.Position), mine.Velocity) * Weight;
        }
    }
}


namespace CloudFine.FlockBox
{
    [DOTSCompatible]
    public partial class SeekBehavior : IConvertToSteeringBehaviorComponentData<SeekData>
    {
        
        public SeekData Convert()
        {
            return new SeekData
            {
                Active = IsActive,
                Weight = weight,
                Radius = effectiveRadius,
                GlobalTagSearch = globalTagSearch,
                TagMask = (useTagFilter ? TagMaskUtility.GetTagMask(filterTags) : int.MaxValue)
            };
        }

        public bool HasEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.HasEntityData(this, entity, entityManager);
        public void AddEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.AddEntityData(this, entity, entityManager);
        public void SetEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.SetEntityData(this, entity, entityManager);
        public void RemoveEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.RemoveEntityData(this, entity, entityManager);
    }
}
#endif