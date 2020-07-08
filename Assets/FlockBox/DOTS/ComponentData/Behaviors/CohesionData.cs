using System;
using Unity.Entities;

public struct CohesionData : IComponentData
{
    public float Radius;
    public Int32 TagMask;
}
