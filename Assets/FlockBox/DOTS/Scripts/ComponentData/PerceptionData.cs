#if FLOCKBOX_DOTS
using Unity.Entities;
using Unity.Mathematics;

namespace CloudFine.FlockBox.DOTS
{
    public struct PerceptionData : IComponentData
    {
        public float perceptionRadius;
        public float lookAheadSeconds;
        public int globalSearchTagMask;
        //not sure how to implement this
        //public List<System.Tuple<Shape, Vector3>> perceptionShapes { get; private set; }

        public void Clear()
        {
            perceptionRadius = 0;
            lookAheadSeconds = 0;
            globalSearchTagMask = 0;
        }

        public void ExpandPerceptionRadius(float radius)
        {
            perceptionRadius = math.max(radius, perceptionRadius);
        }

        public void ExpandLookAheadSeconds(float seconds)
        {
            lookAheadSeconds = math.max(lookAheadSeconds, seconds);
        }

        public void AddGlobalSearchTag(byte tag)
        {
            globalSearchTagMask = globalSearchTagMask | 1 << tag;
        }

        public void AddGlobalSearchTagMask(int mask)
        {
            globalSearchTagMask = globalSearchTagMask | mask;
        }
    }
}
#endif