using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace CloudFine.FlockBox.DOTS
{
    public class CohesionSystem : SteeringBehaviorSystem<CohesionData>
    {

    }

    public struct CohesionData : IComponentData, ISteeringBehaviorComponentData
    {
        public float Weight;
        public float Radius;
        public Int32 TagMask;


        public float3 GetSteering(ref AgentData mine, ref SteeringData steering, ref BoundaryData boundary, DynamicBuffer<NeighborData> neighbors)
        {
            float3 sum = float3.zero;
            float count = 0;
            for (int i = 0; i < neighbors.Length; i++)
            {
                AgentData other = neighbors[i].Value;
                if (global::TagMaskUtility.TagInMask(other.Tag, TagMask))
                {
                    if (math.lengthsq(mine.Position - other.Position) < Radius * Radius)
                    {
                        sum += (other.Position);
                        count++;
                    }
                }
            }

            if (count > 0)
            {
                return steering.GetSeekVector(sum / count, mine.Position, mine.Velocity) * Weight;
            }

            return float3.zero;
        }


        public void AddPerception(ref AgentData mine, ref PerceptionData perception)
        {

        }
    }
}

