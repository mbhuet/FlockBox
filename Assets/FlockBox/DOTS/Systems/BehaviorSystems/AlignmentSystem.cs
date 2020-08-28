using System;
using Unity.Entities;
using Unity.Mathematics;

namespace CloudFine.FlockBox.DOTS
{
    public class AlignmentSystem : SteeringBehaviorSystem<AlignmentData>
    {

    }

    public struct AlignmentData : IComponentData, ISteeringBehaviorComponentData
    {
        public boolean Active;
        public float Weight;
        public float Radius;
        public Int32 TagMask;


        public float3 GetSteering(AgentData mine, SteeringData steering, DynamicBuffer<NeighborData> neighbors)
        {
            if (!Active) return float3.zero;

            float3 sum = float3.zero;
            float count = 0;
            for (int i = 0; i < neighbors.Length; i++)
            {
                AgentData other = neighbors[i].Value;
                if (other.TagInMask(TagMask))
                {
                    if (math.lengthsq(mine.Position - other.Position) < Radius * Radius)
                    {
                        sum += (other.Velocity);
                        count++;
                    }
                }
            }

            if (count > 0)
            {
                return steering.GetSteerVector(sum / count, mine.Velocity) * Weight;
            }

            return float3.zero;
        }

        public PerceptionData AddPerceptionRequirements(AgentData mine, PerceptionData perception)
        {
            perception.ExpandPerceptionRadius(Radius);
            return perception;
        }
    }
}