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