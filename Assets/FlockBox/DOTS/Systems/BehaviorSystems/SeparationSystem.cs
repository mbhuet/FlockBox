using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class SeparationSystem : SteeringBehaviorSystem<SeparationData>
{

}

public struct SeparationData : IComponentData, ISteeringBehaviorComponentData
{
    public float Weight;
    public float Radius;
    public Int32 TagMask;


    public float3 GetSteering(ref AgentData mine, ref SteeringData steering, DynamicBuffer<NeighborData> neighbors)
    {
        float3 sum = float3.zero;
        float count = 0;
        for (int i =0; i<neighbors.Length; i++)
        {
            AgentData other = neighbors[i].Value;
            if (global::TagMaskUtility.TagInMask(other.Tag, TagMask))
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


    public void AddPerception(ref AgentData mine, ref PerceptionData perception)
    {

    }
}
