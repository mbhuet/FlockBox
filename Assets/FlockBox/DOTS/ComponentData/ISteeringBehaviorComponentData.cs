using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace CloudFine.FlockBox.DOTS
{
    public interface ISteeringBehaviorComponentData
    {
        float3 GetSteering(ref AgentData mine, ref SteeringData steering, ref BoundaryData boundary, DynamicBuffer<NeighborData> neighbors);
        void AddPerception(ref AgentData mine, ref PerceptionData perception);
    }
}