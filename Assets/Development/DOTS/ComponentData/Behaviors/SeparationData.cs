using System;
using Unity.Entities;

public struct SeparationData : IComponentData
{
    public float Radius;
    public Int32 TagMask;
}
