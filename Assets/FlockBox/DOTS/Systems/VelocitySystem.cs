using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace CloudFine.FlockBox.DOTS
{
    [UpdateInGroup(typeof(MovementSystemGroup))]
    [UpdateAfter(typeof(AccelerationSystem))]
    public class VelocitySystem : JobComponentSystem
    {
        [BurstCompile]
        struct VelocityJob : IJobForEach<AgentData>
        {
            public float dt;


            public void Execute(ref AgentData c1)
            {
                c1.Position += c1.Velocity * dt;
            }
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            VelocityJob job = new VelocityJob
            {
                dt = Time.DeltaTime
            };
            return job.Schedule(this, inputDeps);
        }
    }
}
