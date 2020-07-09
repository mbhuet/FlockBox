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
    public float3 Dimensions;
    public float Buffer;

    public float3 GetSteering(DynamicBuffer<NeighborData> neighbors)
    {
        return float3.zero;
    }

    public void AddPerception(PerceptionData perception)
    {

    }
}
