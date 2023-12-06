#if FLOCKBOX_DOTS
using Unity.Transforms;

namespace Unity.Entities
{
    [UpdateAfter(typeof(TransformSystemGroup))]
    public partial class SteeringSystemGroup : ComponentSystemGroup
    {
       
    }
}
#endif