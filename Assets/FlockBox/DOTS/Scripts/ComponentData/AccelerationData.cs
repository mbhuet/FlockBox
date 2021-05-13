#if FLOCKBOX_DOTS
using Unity.Entities;
using Unity.Mathematics;

namespace CloudFine.FlockBox.DOTS
{
    public struct AccelerationData : IComponentData
    {
        public float3 Value;
    }
}
#endif