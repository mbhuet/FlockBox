#if FLOCKBOX_DOTS
using Unity.Entities;
using Unity.Jobs;

namespace CloudFine.FlockBox.DOTS
{
    [UpdateInGroup(typeof(MovementSystemGroup))]
    [UpdateAfter(typeof(SteeringAccelerationSystem))]
    public class SteeringVelocitySystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float dt = Time.DeltaTime;
            var velocityJob = Entities.WithAll<SteeringData>().ForEach((ref AgentData agent) =>
            {
                agent.Position += agent.Velocity * dt;
            })
            .ScheduleParallel(Dependency);

            Dependency = velocityJob;
        }
    }
}
#endif