using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SteeringSystem : JobComponentSystem
{
    [BurstCompile]
    struct SteeringJob : IJobForEach<Acceleration, Surroundings>
    {
        public float dt;


        public void Execute(ref Acceleration c0, ref Surroundings c1)
        {
            //look at surroundings
            //modify acceleration

            //c0.Value += c1.Value * dt;
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        SteeringJob job = new SteeringJob
        {
            //pass input data into the job
            dt = Time.deltaTime
        };
        return job.Schedule(this, inputDeps);
    }
}
