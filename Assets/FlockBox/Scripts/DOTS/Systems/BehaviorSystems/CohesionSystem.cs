#if FLOCKBOX_DOTS
using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using CloudFine.FlockBox.DOTS;

namespace CloudFine.FlockBox.DOTS
{
    public class CohesionSystem : SteeringBehaviorSystem<CohesionData>
    {
        protected override JobHandle DoPerception()
        {
            return Entities
                .ForEach((ref PerceptionData perception, in CohesionData cohesion) =>
                {
                    perception.ExpandPerceptionRadius(cohesion.Radius);
                }
                ).ScheduleParallel(Dependency);
        }

        protected override JobHandle DoSteering()
        {
            return Entities
                .ForEach((DynamicBuffer<NeighborData> neighbors, ref AccelerationData acceleration, in AgentData agent, in SteeringData steering, in CohesionData cohesion) =>
                {
                    acceleration.Value += cohesion.CalculateSteering(agent, steering, neighbors);
                }
                ).ScheduleParallel(Dependency);
        }
    }

    public struct CohesionData : IComponentData
    {
        public bool Active;
        public float Weight;
        public float Radius;
        public Int32 TagMask;


        public float3 CalculateSteering(AgentData mine, SteeringData steering, DynamicBuffer<NeighborData> neighbors)
        {
            if (!Active) return float3.zero;

            float3 sum = float3.zero;
            float count = 0;
            for (int i = 0; i < neighbors.Length; i++)
            {
                AgentData other = neighbors[i].Value;

                    if (other.TagInMask(TagMask))
                    {
                    if (!mine.Equals(other))
                    {
                        if (math.lengthsq(mine.Position - other.Position) < Radius * Radius)
                        {
                            sum += (other.Position);
                            count++;
                        }
                    }
                }
            }

            if (count > 0)
            {
                return steering.GetSeekVector(sum / count, mine.Position, mine.Velocity) * Weight;
            }

            return float3.zero;
        }
    }
}


namespace CloudFine.FlockBox
{
    [DOTSCompatible]
    public partial class CohesionBehavior : IConvertToSteeringBehaviorComponentData<CohesionData>
    {
        public CohesionData Convert()
        {
            return new CohesionData
            {
                Active = IsActive,
                Weight = weight,
                Radius = effectiveRadius,
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

