using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct SteeringData : IComponentData
{
    public float MaxSpeed;
    public float MaxForce;

    public float3 GetSeekVector(float3 target, float3 position, float3 velocity)
    {
        return GetSteerVector(target - position, velocity);
    }

    public float3 GetSteerVector(float3 desiredForward, float3 velocity)
    {
        float3 steer = math.normalize(desiredForward) * MaxSpeed - velocity;
        steer = math.normalize(steer) * math.min(math.length(steer), MaxForce);
        return steer;
    }
}
