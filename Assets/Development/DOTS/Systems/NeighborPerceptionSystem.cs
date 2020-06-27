using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


[UpdateInGroup(typeof(PerceptionSystemGroup))]
public class NeighborPerceptionSystem : JobComponentSystem
{
    [BurstCompile]
    struct NeighborPerceptionJob : IJobForEach_BC<NeighborData, PerceptionData>
    {

        public void Execute(DynamicBuffer<NeighborData> b0, ref PerceptionData c1)
        {
            //throw new System.NotImplementedException();
            //find local neighbors, modify b0
            b0.Clear();

            AgentData neighbor = new AgentData { };
            b0.Add(neighbor);
            
            c1.Clear();
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        NeighborPerceptionJob job = new NeighborPerceptionJob
        {

        };
        return job.Schedule(this, inputDeps);
    }
}
