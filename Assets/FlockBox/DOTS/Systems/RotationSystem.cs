using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(MovementSystemGroup))]
[UpdateAfter(typeof(AccelerationSystem))]
public class RotationSystem : JobComponentSystem
{
    [BurstCompile]
    struct VelocityJob : IJobForEach<Rotation, AgentData>
    {
        public float dt;

        public void Execute(ref Rotation c0, ref AgentData c1)
        {
            if (!math.all(c1.Velocity == float3.zero)) {
                c0.Value = quaternion.LookRotation(c1.Velocity, new float3(0, 1, 0));
                c1.Forward = math.normalize(c1.Velocity); 
            } 
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        VelocityJob job = new VelocityJob
        {
            //pass input data into the job
            dt = Time.deltaTime
        };
        return job.Schedule(this, inputDeps);
    }
}
