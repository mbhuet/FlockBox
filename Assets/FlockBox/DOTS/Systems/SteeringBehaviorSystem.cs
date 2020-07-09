using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

[UpdateInGroup(typeof(SteeringSystemGroup))]
public abstract class SteeringBehaviorSystem<T> : JobComponentSystem where T : struct, IComponentData, ISteeringBehaviorComponentData
{
    [BurstCompile]
    protected struct SteeringJob : IJobForEach_BCCC<NeighborData, Acceleration, SteeringData, T>
    {
        public void Execute(DynamicBuffer<NeighborData> b0, ref Acceleration c1, ref SteeringData c2, ref T c3)
        {
            float3 steer = c3.GetSteering(b0);
            //cap with SteeringData
            c1.Value += steer;
        }
    }

    [BurstCompile]
    protected struct PerceptionJob : IJobForEach<PerceptionData, T>
    {

        public void Execute(ref PerceptionData c1, ref T c0)
        {
            c0.AddPerception(c1);
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