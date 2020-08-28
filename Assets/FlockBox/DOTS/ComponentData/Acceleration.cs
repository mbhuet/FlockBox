using Unity.Entities;
using Unity.Mathematics;

namespace CloudFine.FlockBox.DOTS
{
    public struct Acceleration : IComponentData
    {
        public float3 Value;
    }
}