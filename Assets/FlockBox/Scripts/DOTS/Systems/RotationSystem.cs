#if FLOCKBOX_DOTS
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


namespace CloudFine.FlockBox.DOTS
{
    [UpdateInGroup(typeof(MovementSystemGroup))]
    [UpdateAfter(typeof(AccelerationSystem))]
    public class RotationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float dt = Time.DeltaTime;
            var rotationJob = Entities.ForEach((ref AgentData agent, ref Rotation rot) =>
            {
                if (!math.all(agent.Velocity == float3.zero))
                {
                    rot.Value = quaternion.LookRotationSafe(agent.Velocity, new float3(0, 1, 0));
                    agent.Forward = math.normalize(agent.Velocity);
                }
            })
            .ScheduleParallel(Dependency);

            Dependency = rotationJob;
        }
    }
}
#endif