using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace CloudFine.FlockBox.DOTS
{
    public class SeparationSystem : SteeringBehaviorSystem<SeparationData>
    {
        protected override JobHandle DoPerception()
        {
            return Entities.ForEach((ref PerceptionData perception, in SeparationData separation) =>
            {
                perception.ExpandPerceptionRadius(separation.Radius);
            }).ScheduleParallel(Dependency);
        }

        protected override JobHandle DoSteering()
        {
            return Entities
                .ForEach((DynamicBuffer<NeighborData> neighbors, ref Acceleration acceleration, in AgentData agent, in SteeringData steering, in SeparationData separation) =>
                {
                    acceleration.Value += separation.CalculateSteering(agent, steering, neighbors);
                }).ScheduleParallel(Dependency);
        }
    }

    public struct SeparationData : IComponentData
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
                        float3 diff = mine.Position - other.Position;
                        float dist = math.length(diff);
                        if (dist < Radius)
                        {
                            if (dist > .001f)
                            {
                                sum += (math.normalize(diff) / dist);
                                count++;
                            }
                        }
                    }
                }
            }
            if (count > 0)
            {
                return steering.GetSteerVector(sum / count, mine.Velocity) * Weight;
            }

            return float3.zero;
        }

    }
}
