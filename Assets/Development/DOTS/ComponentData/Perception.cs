using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct Perception : IComponentData
{
    float perceptionRadius;
    float lookAheadSeconds;
    //bitmask for tags
    Int32 globalSearchTagMask;
    //not sure how to implement this
    //public List<System.Tuple<Shape, Vector3>> perceptionShapes { get; private set; }
}
