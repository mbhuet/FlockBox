using CloudFine;
using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace CloudFine.FlockBox {
    public struct FlockData : ISharedComponentData, IEquatable<FlockData>
    {
        public FlockBox Flock;

        public bool Equals(FlockData other)
        {
            return other.Flock == Flock;
        }


        public override int GetHashCode()
        {
            int hash = 0;

            if (!ReferenceEquals(Flock, null))
                hash ^= Flock.GetHashCode();

            return hash;
        }
    }
}
