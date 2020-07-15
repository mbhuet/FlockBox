using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;


namespace CloudFine.FlockBox.DOTS
{
    [UpdateInGroup(typeof(SteeringSystemGroup))]
    public abstract class SteeringBehaviorSystem<T> : JobComponentSystem where T : struct, IComponentData, ISteeringBehaviorComponentData
    {
        [BurstCompile]
        protected struct SteeringJob : IJobForEach_BCCCCC<NeighborData, AgentData, Acceleration, SteeringData, BoundaryData, T>
        {
            public void Execute(DynamicBuffer<NeighborData> neighbors, ref AgentData agent, ref Acceleration accel, ref SteeringData steering, ref BoundaryData boundary, ref T behavior)
            {
                accel.Value += behavior.GetSteering(ref agent, ref steering, ref boundary, neighbors);
            }
        }

        [BurstCompile]
        protected struct PerceptionJob : IJobForEach<PerceptionData, AgentData, T>
        {
            public void Execute(ref PerceptionData perception, ref AgentData agent, ref T behavior)
            {
                behavior.AddPerceptionRequirements(ref agent, ref perception);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            PerceptionJob perceptJob = new PerceptionJob
            {
            };
            inputDeps = perceptJob.Schedule(this, inputDeps);

            SteeringJob job = new SteeringJob
            {
            };
            return job.Schedule(this, inputDeps);
        }
    }
}