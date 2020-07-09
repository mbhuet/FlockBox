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


    public float3 GetSteering(DynamicBuffer<NeighborData> neighbors)
    {
        return new float3(0, 1, 0);
    }


    public void AddPerception(PerceptionData perception)
    {

    }
}