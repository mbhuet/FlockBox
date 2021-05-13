#if FLOCKBOX_DOTS
using System;
using Unity.Entities;

namespace CloudFine.FlockBox.DOTS {
    public struct BehaviorSettingsData : ISharedComponentData, IEquatable<BehaviorSettingsData>
    {
        public BehaviorSettings Settings;

        public bool Equals(BehaviorSettingsData other)
        {
            return other.Settings == Settings;
        }


        public override int GetHashCode()
        {
            int hash = 0;

            if (!ReferenceEquals(Settings, null))
                hash ^= Settings.GetHashCode();

            return hash;
        }
    }
}
#endif