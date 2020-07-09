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
    public float Radius;
    public Int32 TagMask;

    public float3 GetSteering(DynamicBuffer<NeighborData> neighbors)
    {
        return float3.zero;
    }


    public void AddPerception(PerceptionData perception)
    {

    }
}
