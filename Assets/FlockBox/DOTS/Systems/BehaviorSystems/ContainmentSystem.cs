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
    public float LookAheadSeconds;

    public float3 GetSteering(ref AgentData mine, ref SteeringData steering, ref BoundaryData boundary, DynamicBuffer<NeighborData> neighbors)
    {
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


    public void AddPerception(ref AgentData mine, ref PerceptionData perception)
    {

    }
}
