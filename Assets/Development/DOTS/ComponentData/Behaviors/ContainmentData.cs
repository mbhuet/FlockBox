using Unity.Entities;
using Unity.Mathematics;

public struct ContainmentData : IComponentData
{
    public float3 Dimensions;
    public float Buffer;
}
