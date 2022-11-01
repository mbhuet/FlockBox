#if FLOCKBOX_DOTS
using System;
using Unity.Entities;

namespace CloudFine.FlockBox.DOTS {
    public struct BehaviorSettingsData : ISharedComponentData, IEquatable<BehaviorSettingsData>
    {
        public int SettingsInstanceID;

        public bool Equals(BehaviorSettingsData other)
        {
            return other.SettingsInstanceID == SettingsInstanceID;
        }


        public override int GetHashCode()
        {
            return SettingsInstanceID;
        }
    }
}
#endif