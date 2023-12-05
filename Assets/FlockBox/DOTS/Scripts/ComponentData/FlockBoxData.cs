#if FLOCKBOX_DOTS
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace CloudFine.FlockBox.DOTS
{
    public struct FlockBoxData : IComponentData
    {
        public float3 WorldDimensions;
        public float boundaryBuffer;
        public bool wrapEdges;
        public int FlockBoxID;
        public float4x4 WorldToLocalMatrix;
    }
}
#endif