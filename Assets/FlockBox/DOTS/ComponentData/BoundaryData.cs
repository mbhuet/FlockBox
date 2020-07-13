using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct BoundaryData : IComponentData
{
    public float3 Dimensions;
    public float Margin;
}
