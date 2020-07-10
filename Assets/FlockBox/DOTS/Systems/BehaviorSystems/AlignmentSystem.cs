using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class AlignmentSystem : SteeringBehaviorSystem<AlignmentData>
{
    
}

public struct AlignmentData : IComponentData, ISteeringBehaviorComponentData
{
    public float Radius;
    public Int32 TagMask;


    public float3 GetSteering(ref AgentData mine, ref SteeringData steering, DynamicBuffer<NeighborData> neighbors)
    {
        float3 sum = float3.zero;
        float count = 0;
        for (int i = 0; i < neighbors.Length; i++)
        {
            AgentData other = neighbors[i].Value;
            if (global::TagMaskUtility.TagInMask(other.Tag, TagMask))
            {
                if (math.lengthsq(mine.Position - other.Position)< Radius * Radius)
                {
                    sum += (other.Velocity);
                    count++;
                }
            }
            
        }
        if (count > 0)
        {
            return steering.GetSteerVector(sum/count, mine.Velocity);
        }

        return float3.zero;
        
    }


    public void AddPerception(ref AgentData mine, ref PerceptionData perception)
    {

    }
}