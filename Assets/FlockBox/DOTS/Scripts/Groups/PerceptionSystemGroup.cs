#if FLOCKBOX_DOTS
namespace Unity.Entities
{
    [UpdateBefore(typeof(SteeringSystemGroup))]
    public partial class PerceptionSystemGroup : ComponentSystemGroup
    {
       
    }
}
#endif