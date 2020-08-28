using System;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;

namespace CloudFine.FlockBox.DOTS
{
    public class SeekSystem : SteeringBehaviorSystem<SeekData>
    {

    }

    public struct SeekData : IComponentData, ISteeringBehaviorComponentData
    {
        public boolean Active;
        public float Weight;
        public float Radius;
        public Int32 TagMask;
        public bool GlobalTagSearch;

        //TODO keep track for current target for later retrieval
        public float3 GetSteering(AgentData mine, SteeringData steering, DynamicBuffer<NeighborData> neighbors)
        {
            if (!Active) return float3.zero;


            if (neighbors.Length == 0)
            {
                return float3.zero;
            }
            float closeSqrDist = float.MaxValue;
            bool foundTarget = false;
            AgentData closeTarget = new AgentData
            {
                Position = float3.zero
            };
            for (int i =0; i<neighbors.Length; i++)
            {
                AgentData target = neighbors[i];
                if (target.TagInMask(TagMask)) {
                    float sqrDist = math.lengthsq(target.Position - mine.Position);
                    if (sqrDist < closeSqrDist && sqrDist < Radius * Radius)
                    {
                        closeSqrDist = sqrDist;
                        closeTarget = target;
                        foundTarget = true;
                    }
                }
            }

            if (!foundTarget)
            {
                return float3.zero;
            }

            return steering.GetSteerVector((closeTarget.Position - mine.Position), mine.Velocity) * Weight;
        }


        public PerceptionData AddPerceptionRequirements(AgentData mine, PerceptionData perception)
        {
            if (GlobalTagSearch)
            {
                perception.AddGlobalSearchTagMask(TagMask);
            }
            else
            {
                perception.ExpandPerceptionRadius(Radius);
            }

            return perception;
        }
    }
}