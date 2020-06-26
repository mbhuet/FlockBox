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
    struct AlignmentJob : IJobForEachWithEntity<Acceleration, AlignmentData>
    {
        public float dt;
        // Allow buffer read write in parralel jobs
        // Ensure, no two jobs can write to same entity, at the same time.
        // !! "You are somehow completely certain that there is no race condition possible here, because you are absolutely certain that you will not be writing to the same Entity ID multiple times from your parallel for job. (If you do thats a race condition and you can easily crash unity, overwrite memory etc) If you are indeed certain and ready to take the risks.
        // https://forum.unity.com/threads/how-can-i-improve-or-jobify-this-system-building-a-list.547324/#post-3614833
        [NativeDisableParallelForRestriction]
        public BufferFromEntity<SurroundingsData> surBuffer;


        public void Execute(Entity entity, int index, ref Acceleration c0, ref AlignmentData c1)
        {
            DynamicBuffer<SurroundingsData> sur = surBuffer[entity];
            
            //throw new System.NotImplementedException();
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        

        AlignmentJob job = new AlignmentJob
        {
            //pass input data into the job
            dt = Time.deltaTime,
            surBuffer = GetBufferFromEntity<SurroundingsData>(false)

        };
        return job.Schedule(this, inputDeps);
    }
}
