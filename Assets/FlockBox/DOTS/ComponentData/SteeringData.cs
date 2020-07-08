using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct SteeringData : IComponentData
{
    public float MaxSpeed;
    public float MaxForce;

}
