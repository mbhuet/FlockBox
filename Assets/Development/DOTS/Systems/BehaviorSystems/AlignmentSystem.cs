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
    struct AlignmentJob : IJobForEachWithEntity<Acceleration, AlignmentData>
    {
        public float dt;
        public SurroundingsData sur;



        public void Execute(Entity entity, int index, ref Acceleration c0, ref AlignmentData c1)
        {
            //throw new System.NotImplementedException();
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        

        AlignmentJob job = new AlignmentJob
        {
            //pass input data into the job
            dt = Time.deltaTime,
            sur = GetBufferFromEntity<SurroundingsData>()[entity]

    };
        return job.Schedule(this, inputDeps);
    }
}
