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
public class VelocitySystem : JobComponentSystem
{
    //what does a flocksystem need to make decisions about how an agents data should change
    //surroundings
    //settings, list of behaviors
    //
    [BurstCompile]
    //[RequireComponentTag(typeof(AgentTag))] //only look for 
    struct VelocityJob : IJobForEach<Translation, Velocity>
    {
        public float dt;


        public void Execute(ref Translation c0, ref Velocity c1)
        {
            c0.Value += c1.Value * dt;
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
