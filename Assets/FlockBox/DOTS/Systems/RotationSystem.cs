using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


namespace CloudFine.FlockBox.DOTS
{
    [UpdateInGroup(typeof(MovementSystemGroup))]
    [UpdateAfter(typeof(AccelerationSystem))]
    public class RotationSystem : JobComponentSystem
    {
        [BurstCompile]
        struct RotationJob : IJobForEach<Rotation, AgentData>
        {
            public float dt;

            public void Execute(ref Rotation c0, ref AgentData c1)
            {
                if (!math.all(c1.Velocity == float3.zero))
                {
                    c0.Value = quaternion.LookRotationSafe(c1.Velocity, new float3(0, 1, 0));
                    c1.Forward = math.normalize(c1.Velocity);
                }
            }
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            RotationJob job = new RotationJob
            {
                dt = Time.DeltaTime
            };
            return job.Schedule(this, inputDeps);
        }
    }
}
