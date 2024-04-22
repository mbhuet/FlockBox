#if FLOCKBOX_DOTS
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace CloudFine.FlockBox.DOTS
{
    /// <summary>
    /// Holds information about a starting population in ECS environment.
    /// </summary>
    public struct FlockBoxSpawnPopulationData : IBufferElementData
    {
        public Entity Prefab;
        public int Population;
        public float MaxSpeed;
    }
}
#endif