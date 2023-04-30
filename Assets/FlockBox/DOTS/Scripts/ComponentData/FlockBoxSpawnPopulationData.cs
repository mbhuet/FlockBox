#if FLOCKBOX_DOTS
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace CloudFine.FlockBox.DOTS
{
    public struct FlockBoxSpawnPopulationData : IBufferElementData
    {
        public Entity Prefab;
        public int Population;
    }
}
#endif