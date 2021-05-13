#if FLOCKBOX_DOTS
using Unity.Entities;

namespace CloudFine.FlockBox.DOTS
{
    // This describes the number of buffer elements that should be reserved
    // in chunk data for each instance of a buffer. In this case, 8 integers
    // will be reserved (32 bytes) along with the size of the buffer header
    // (currently 16 bytes on 64-bit targets)
    [InternalBufferCapacity(12)]
    public struct NeighborData : IBufferElementData
    {
        // These implicit conversions are optional, but can help reduce typing.
        public static implicit operator AgentData(NeighborData e) { return e.Value; }
        public static implicit operator NeighborData(AgentData e) { return new NeighborData { Value = e }; }

        // Actual value each buffer element will store.
        public AgentData Value;
    }
}
#endif