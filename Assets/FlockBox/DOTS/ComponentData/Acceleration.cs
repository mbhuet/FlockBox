using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct Acceleration : IComponentData
{
    public float3 Value;
}
