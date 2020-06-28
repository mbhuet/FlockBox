using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(MovementSystemGroup))]
public class AccelerationSystem : JobComponentSystem
{
    //what does a flocksystem need to make decisions about how an agents data should change
    //surroundings
    //settings, list of behaviors
    //
    [BurstCompile]
    //[RequireComponentTag(typeof(AgentTag))] //only look for 
    struct AccelerationJob : IJobForEach<AgentData, Acceleration>
    {
        public float dt;


        public void Execute(ref AgentData vel, ref Acceleration accel)
        {
            vel.Velocity += accel.Value * dt;
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        AccelerationJob job = new AccelerationJob
        {
            //pass input data into the job
            dt = Time.deltaTime
        };
        return job.Schedule(this, inputDeps);
    }
}
