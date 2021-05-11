#if FLOCKBOX_DOTS
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using CloudFine.FlockBox.DOTS;

namespace CloudFine.FlockBox.DOTS
{
    public class ContainmentSystem : SteeringBehaviorSystem<ContainmentData>
    {

        protected override JobHandle DoPerception()
        {
            return Dependency;
        }

        protected override JobHandle DoSteering()
        {
            return Entities
                .ForEach((ref AccelerationData acceleration, in AgentData agent, in ContainmentData behavior, in SteeringData steering, in BoundaryData boundary) =>
                {
                    acceleration.Value += behavior.CalculateSteering(agent, steering, boundary);

                }
                ).ScheduleParallel(Dependency);
        }

    }

    public struct ContainmentData : IComponentData
    {
        public float Weight;
        public float LookAheadSeconds;

        public float3 CalculateSteering(AgentData mine, SteeringData steering, BoundaryData boundary)
        {
            if (boundary.Wrap) return float3.zero;

            float3 unclampedFuturePosition = mine.Position + mine.Velocity * LookAheadSeconds;
            float3 containedPosition = unclampedFuturePosition;

            float distanceToBorder = float.MaxValue;

            if (boundary.Dimensions.x > 0)
            {
                distanceToBorder = math.min(distanceToBorder, math.min(mine.Position.x, boundary.Dimensions.x - mine.Position.x));
                containedPosition.x = math.clamp(containedPosition.x, boundary.Margin, boundary.Dimensions.x - boundary.Margin);
            }
            else
            {
                containedPosition.x = 0;
            }

            if (boundary.Dimensions.y > 0)
            {
                distanceToBorder = math.min(distanceToBorder, math.min(mine.Position.y, boundary.Dimensions.y - mine.Position.y));
                containedPosition.y = math.clamp(containedPosition.y, boundary.Margin, boundary.Dimensions.y - boundary.Margin);
            }
            else
            {
                containedPosition.y = 0;
            }

            if (boundary.Dimensions.z > 0)
            {
                distanceToBorder = math.min(distanceToBorder, math.min(mine.Position.z, boundary.Dimensions.z - mine.Position.z));
                containedPosition.z = math.clamp(containedPosition.z, boundary.Margin, boundary.Dimensions.z - boundary.Margin);
            }
            else
            {
                containedPosition.z = 0;
            }

            if (math.all(containedPosition == unclampedFuturePosition))
            {
                return float3.zero;
            }

            if (distanceToBorder <= 0) distanceToBorder = .001f;

            return steering.GetSeekVector(containedPosition, mine.Position, mine.Velocity) * (boundary.Margin / distanceToBorder) * Weight;
        }
    }
}


namespace CloudFine.FlockBox
{
    [DOTSCompatible]
    public partial class ContainmentBehavior : IConvertToSteeringBehaviorComponentData<ContainmentData>
    {
        public ContainmentData Convert()
        {
            return new ContainmentData { Weight = weight, LookAheadSeconds = lookAheadSeconds };
        }

        public bool HasEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.HasEntityData(this, entity, entityManager);
        public void AddEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.AddEntityData(this, entity, entityManager);
        public void SetEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.SetEntityData(this, entity, entityManager);
        public void RemoveEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.RemoveEntityData(this, entity, entityManager);
    }

}
#endif