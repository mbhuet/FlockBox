#if FLOCKBOX_DOTS
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

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
                if (!math.all(agent.Velocity == float3.zero))
                {
                    agent.Forward = math.normalize(agent.Velocity);
                }
            })
            .ScheduleParallel(Dependency);

            Dependency = velocityJob;
        }
    }
}
#endif