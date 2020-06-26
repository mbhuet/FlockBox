using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct SeparationData : IComponentData
{
    public float Radius;
    public Int32 TagMask;
}
