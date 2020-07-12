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
    public float Buffer;

    public float3 GetSteering(ref AgentData mine, ref SteeringData steering, DynamicBuffer<NeighborData> neighbors)
    {
        return float3.zero * Weight;
    }

    public void AddPerception(ref AgentData mine, ref PerceptionData perception)
    {

    }
}
