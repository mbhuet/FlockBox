using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct AlignmentData : IComponentData
{
    public float Radius;
    public Int32 TagMask;
}

