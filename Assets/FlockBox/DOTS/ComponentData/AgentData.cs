using Unity.Entities;
using Unity.Mathematics;

namespace CloudFine.FlockBox.DOTS
{
    public struct AgentData : IComponentData
    {
        public byte Tag;
        public float Radius;

        public bool Fill;

        public float3 Position;
        public float3 Velocity;
        public float3 Forward;

        public bool Sleeping;

        public bool TagInMask(int mask)
        {
            return ((1 << Tag & mask) != 0);
        }
    }
}