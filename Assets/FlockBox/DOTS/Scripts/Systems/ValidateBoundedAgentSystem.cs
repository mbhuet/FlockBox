#if FLOCKBOX_DOTS
using Unity.Entities;
using Unity.Jobs;


namespace CloudFine.FlockBox.DOTS
{
    [UpdateInGroup(typeof(MovementSystemGroup))]
    [UpdateAfter(typeof(SteeringAccelerationSystem)), UpdateBefore(typeof(SteeringVelocitySystem))]
    public class ValidateBoundedAgentSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var validationJob = Entities.ForEach((ref AgentData agent, in BoundaryData boundary) =>
            {
                agent.Position = boundary.ValidatePosition(agent.Position);
                agent.Velocity = boundary.ValidateDirection(agent.Velocity);
            })
            .ScheduleParallel(Dependency);

            Dependency = validationJob;
        }
    }
}
#endif
