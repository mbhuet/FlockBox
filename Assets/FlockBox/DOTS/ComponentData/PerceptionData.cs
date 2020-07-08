using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct PerceptionData : IComponentData
{
    public float perceptionRadius;
    public float lookAheadSeconds;
    //bitmask for tags
    public Int32 globalSearchTagMask;
    //not sure how to implement this
    //public List<System.Tuple<Shape, Vector3>> perceptionShapes { get; private set; }

    public void Clear()
    {
        perceptionRadius = 0;
        lookAheadSeconds = 0;
        globalSearchTagMask = 0;
    }
}
