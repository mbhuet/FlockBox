using System;
using Unity.Entities;
using Unity.Mathematics;

namespace CloudFine.FlockBox.DOTS
{
    public class SeparationSystem : SteeringBehaviorSystem<SeparationData>
    {

    }

    public struct SeparationData : IComponentData, ISteeringBehaviorComponentData
    {
        public bool Active;
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
                        float3 diff = mine.Position - other.Position;

                        //need to filter out "mine"
                        /*
                        if (math.lengthsq(diff) < .001f)
                        {
                            Unity.Mathematics.Random rand = new Unity.Mathematics.Random();
                            diff = new float3(rand.NextFloat(1), rand.NextFloat(1), rand.NextFloat(1)) * .01f;
                        }
                        */
                        if (math.lengthsq(diff) > .001f)
                        {
                            sum += (math.normalize(diff) / math.length(diff));
                            count++;
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


        public PerceptionData AddPerceptionRequirements(AgentData mine, PerceptionData perception)
        {
            perception.ExpandPerceptionRadius(Radius);
            return perception;
        }
    }
}
