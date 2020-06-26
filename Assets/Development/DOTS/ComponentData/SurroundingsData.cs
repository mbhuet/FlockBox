﻿using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// This describes the number of buffer elements that should be reserved
// in chunk data for each instance of a buffer. In this case, 8 integers
// will be reserved (32 bytes) along with the size of the buffer header
// (currently 16 bytes on 64-bit targets)
[InternalBufferCapacity(12)]
public struct SurroundingsData : IBufferElementData
{
    // These implicit conversions are optional, but can help reduce typing.
    public static implicit operator AgentData(SurroundingsData e) { return e.Value; }
    public static implicit operator SurroundingsData(AgentData e) { return new SurroundingsData { Value = e }; }

    // Actual value each buffer element will store.
    public AgentData Value;
}