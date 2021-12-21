#if FLOCKBOX_DOTS
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace CloudFine.FlockBox.DOTS
{
    [UpdateInGroup(typeof(MovementSystemGroup))]
    public class SteeringAccelerationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float dt = Time.DeltaTime;
            var accelerationJob = Entities.ForEach((ref AgentData agent, ref AccelerationData accel, in SteeringData steer) =>
            {
                agent.Velocity += accel.Value * dt;
                agent.Velocity = math.normalize(agent.Velocity) * math.min(math.length(agent.Velocity), steer.MaxSpeed);
                accel.Value = float3.zero;
            })
            .ScheduleParallel(Dependency);

            Dependency = accelerationJob;
        }
    }
}
#endif