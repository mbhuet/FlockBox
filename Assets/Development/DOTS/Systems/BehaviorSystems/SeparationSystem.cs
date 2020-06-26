using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SteeringSystemGroup))]
public abstract class SeparationSystem : JobComponentSystem
{
    [BurstCompile]
    struct SeparationJob : IJobForEach<Acceleration, Surroundings, SeparationData>
    {
        public float dt;


        public void Execute(ref Acceleration c0, ref Surroundings c1, ref SeparationData sep)
        {
            //look at surroundings
            //modify acceleration

            //c0.Value += c1.Value * dt;
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        SeparationJob job = new SeparationJob
        {
            //pass input data into the job
            dt = Time.deltaTime
        };
        return job.Schedule(this, inputDeps);
    }
}
