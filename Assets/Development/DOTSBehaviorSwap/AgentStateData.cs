#if FLOCKBOX_DOTS
using Unity.Entities;
using Unity.Mathematics;

namespace CloudFine.FlockBox.DOTS
{
    public struct AgentStateData : IComponentData
    {
        public int State;
    }
}
#endif