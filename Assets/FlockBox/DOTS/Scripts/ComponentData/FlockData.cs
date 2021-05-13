#if FLOCKBOX_DOTS
using System;
using Unity.Entities;

namespace CloudFine.FlockBox.DOTS {
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
#endif