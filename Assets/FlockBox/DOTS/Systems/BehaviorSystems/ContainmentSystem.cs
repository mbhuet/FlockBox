using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class ContainmentSystem : SteeringBehaviorSystem<ContainmentData>
{

}

public struct ContainmentData : IComponentData, ISteeringBehaviorComponentData
{
    public float Weight;
    public float3 Dimensions;
    public float Margin;
    public float LookAheadSeconds;

    public float3 GetSteering(ref AgentData mine, ref SteeringData steering, DynamicBuffer<NeighborData> neighbors)
    {
        float3 unclampedFuturePosition = mine.Position + mine.Velocity * LookAheadSeconds;
        float3 containedPosition = unclampedFuturePosition;

        float distanceToBorder = float.MaxValue;

        if (Dimensions.x > 0)
        {
            distanceToBorder = math.min(distanceToBorder, math.min(mine.Position.x, Dimensions.x - mine.Position.x));
            containedPosition.x = math.clamp(containedPosition.x, Margin, Dimensions.x - Margin);
        }
        else
        {
            containedPosition.x = 0;
        }

        if (Dimensions.y > 0)
        {
            distanceToBorder = math.min(distanceToBorder, math.min(mine.Position.y, Dimensions.y - mine.Position.y));
            containedPosition.y = math.clamp(containedPosition.y, Margin, Dimensions.y - Margin);
        }
        else
        {
            containedPosition.y = 0;
        }

        if (Dimensions.z > 0)
        {
            distanceToBorder = math.min(distanceToBorder, math.min(mine.Position.z, Dimensions.z - mine.Position.z));
            containedPosition.z = math.clamp(containedPosition.z, Margin, Dimensions.z - Margin);
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

        return steering.GetSeekVector(containedPosition, mine.Position, mine.Velocity) * (Margin / distanceToBorder) * Weight;
    }


    public void AddPerception(ref AgentData mine, ref PerceptionData perception)
    {

    }
}
