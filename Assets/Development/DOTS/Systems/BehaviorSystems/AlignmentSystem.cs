using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

[UpdateInGroup(typeof(SteeringSystemGroup))]

public abstract class AlignmentSystem : SteeringSystem
{
    [BurstCompile]
    struct AlignmentJob : IJobForEach<Acceleration, Surroundings, AlignmentData>
    {
        public float dt;


        public void Execute(ref Acceleration accel, ref Surroundings sur, ref AlignmentData align)
        {
            //look at surroundings
            //modify acceleration

            //c0.Value += c1.Value * dt;
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        AlignmentJob job = new AlignmentJob
        {
            //pass input data into the job
            dt = Time.deltaTime
        };
        return job.Schedule(this, inputDeps);
    }
}
