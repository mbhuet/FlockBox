#if FLOCKBOX_DOTS
using System;
using Unity.Entities;

namespace CloudFine.FlockBox.DOTS {
    public struct FlockData : ISharedComponentData, IEquatable<FlockData>
    {
        public int FlockInstanceID;

        public bool Equals(FlockData other)
        {
            return other.FlockInstanceID == FlockInstanceID;
        }

        public override int GetHashCode()
        {
            return FlockInstanceID;
        }
    }
}
#endif