#if FLOCKBOX_DOTS
using Unity.Entities;
using Unity.Mathematics;

namespace CloudFine.FlockBox.DOTS
{
    public struct FlockMatrixData : IComponentData
    {
        public float4x4 WorldToFlockMatrix;
    }
}
#endif