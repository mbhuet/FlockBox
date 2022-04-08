#if FLOCKBOX_DOTS
using Unity.Entities;
using Unity.Mathematics;

namespace CloudFine.FlockBox.DOTS
{
    public struct FlockMatrixData : IComponentData
    {
        public float4x4 WorldToFlockMatrix;
        public float4x4 FlockToWorldMatrix => math.inverse(WorldToFlockMatrix);

        public float3 WorldToFlockPoint(float3 worldPoint)
        {
            return math.transform(WorldToFlockMatrix, worldPoint);
        }

        public float3 FlockToWorldPoint(float3 flockPoint)
        {
            return math.transform(FlockToWorldMatrix, flockPoint);
        }

        public float3 WorldToFlockDirection(float3 worldDir)
        {
            return math.rotate(WorldToFlockMatrix, worldDir);
        }

        public float3 FlockToWorldDirection(float3 flockDir)
        {
            return math.rotate(FlockToWorldMatrix, flockDir);
        }
    }
}
#endif