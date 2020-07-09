using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public interface ISteeringBehaviorComponentData
{
    float3 GetSteering(DynamicBuffer<NeighborData> neighbors);
    void AddPerception(PerceptionData perception);
}
