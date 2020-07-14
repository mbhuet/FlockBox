using CloudFine;
using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace CloudFine.FlockBox {
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
