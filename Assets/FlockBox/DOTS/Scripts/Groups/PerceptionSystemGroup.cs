#if FLOCKBOX_DOTS
namespace Unity.Entities
{
    [UpdateBefore(typeof(SteeringSystemGroup))]
    public class PerceptionSystemGroup : ComponentSystemGroup
    {
       
    }
}
#endif