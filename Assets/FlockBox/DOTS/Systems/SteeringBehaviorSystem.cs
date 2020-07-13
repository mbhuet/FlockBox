using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Experimental.XR;

[UpdateInGroup(typeof(SteeringSystemGroup))]
public abstract class SteeringBehaviorSystem<T> : JobComponentSystem where T : struct, IComponentData, ISteeringBehaviorComponentData
{
    [BurstCompile]
    protected struct SteeringJob : IJobForEach_BCCCCC<NeighborData, AgentData, Acceleration, SteeringData, BoundaryData, T>
    {
        public void Execute(DynamicBuffer<NeighborData> b0, ref AgentData agent, ref Acceleration accel, ref SteeringData steering, ref BoundaryData boundary, ref T c3)
        {
            float3 steer = c3.GetSteering(ref agent, ref steering, ref boundary, b0);
            //cap with SteeringData
            accel.Value += steer;
        }
    }

    [BurstCompile]
    protected struct PerceptionJob : IJobForEach<PerceptionData, AgentData, T>
    {
        public void Execute(ref PerceptionData c1, ref AgentData c2, ref T c0)
        {
            c0.AddPerception(ref c2,ref c1);
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