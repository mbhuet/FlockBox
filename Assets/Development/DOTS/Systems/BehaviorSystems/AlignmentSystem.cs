using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
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
    struct AlignmentJob : IJobForEachWithEntity_EBCC<SurroundingsData, Acceleration, AlignmentData>
    {
        public float dt;
        // Allow buffer read write in parralel jobs
        // Ensure, no two jobs can write to same entity, at the same time.
        // !! "You are somehow completely certain that there is no race condition possible here, because you are absolutely certain that you will not be writing to the same Entity ID multiple times from your parallel for job. (If you do thats a race condition and you can easily crash unity, overwrite memory etc) If you are indeed certain and ready to take the risks.
        // https://forum.unity.com/threads/how-can-i-improve-or-jobify-this-system-building-a-list.547324/#post-3614833
        [NativeDisableParallelForRestriction]
        public BufferFromEntity<SurroundingsData> surBuffer;


        public void Execute(Entity entity, int index, DynamicBuffer<SurroundingsData> b0, ref Acceleration c1, ref AlignmentData c2)
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

        };
        return job.Schedule(this, inputDeps);
    }
}
