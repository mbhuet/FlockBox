using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace CloudFine.FlockBox.DOTS
{
    [UpdateInGroup(typeof(MovementSystemGroup))]
    [UpdateAfter(typeof(AccelerationSystem))]
    public class VelocitySystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float dt = Time.DeltaTime;
            var velocityJob = Entities.ForEach((ref AgentData agent) =>
            {
                agent.Position += agent.Velocity * dt;
            })
            .ScheduleParallel(Dependency);

            Dependency = velocityJob;
        }
    }
}
