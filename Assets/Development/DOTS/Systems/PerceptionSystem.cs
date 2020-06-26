using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(SteeringSystemGroup))]
public class PerceptionSystem : JobComponentSystem
{
    //what does a flocksystem need to make decisions about how an agents data should change
    //surroundings
    //settings, list of behaviors
    //
    [BurstCompile]
    //[RequireComponentTag(typeof(AgentTag))] //only look for 
    struct PerceptionJob : IJobForEach<Perception, NeighborData>
    {

        public void Execute(ref Perception per, ref NeighborData c0)
        {
            //c0.Value += c1.Value * dt;
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        PerceptionJob job = new PerceptionJob
        {
            //pass input data into the job
            //dt = Time.deltaTime
        };
        return job.Schedule(this, inputDeps);
    }
}
