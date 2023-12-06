#if FLOCKBOX_DOTS
namespace Unity.Entities
{
    [UpdateAfter(typeof(SteeringSystemGroup))]
    public partial class MovementSystemGroup : ComponentSystemGroup
    {
       
    }
}
#endif