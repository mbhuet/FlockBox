using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


namespace CloudFine.FlockBox.DOTS
{

    [UpdateInGroup(typeof(MovementSystemGroup))]
    public class AccelerationSystem : JobComponentSystem
    {
        [BurstCompile]
        struct AccelerationJob : IJobForEach<AgentData, SteeringData, Acceleration>
        {
            public float dt;

            public void Execute(ref AgentData agent, ref SteeringData steer, ref Acceleration accel)
            {
                agent.Velocity += accel.Value * dt;
                agent.Velocity = math.normalize(agent.Velocity) * math.min(math.length(agent.Velocity), steer.MaxSpeed);
                accel.Value = float3.zero;
            }
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            AccelerationJob job = new AccelerationJob
            {
                //pass input data into the job
                dt = Time.DeltaTime
            };
            return job.Schedule(this, inputDeps);
        }
    }
}
