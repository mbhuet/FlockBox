#if FLOCKBOX_DOTS
namespace Unity.Entities
{
    [UpdateAfter(typeof(SteeringSystemGroup))]
    public class MovementSystemGroup : ComponentSystemGroup
    {
       
    }
}
#endif