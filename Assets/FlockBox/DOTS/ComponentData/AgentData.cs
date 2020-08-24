using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct AgentData : IComponentData
{
    public byte Tag;
    public float Radius;

    public bool Fill;

    public float3 Position;
    public float3 Velocity;
    public float3 Forward;

}
