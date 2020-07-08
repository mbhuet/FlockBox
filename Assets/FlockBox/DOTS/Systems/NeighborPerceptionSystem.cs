using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


[UpdateInGroup(typeof(PerceptionSystemGroup))]
public class NeighborPerceptionSystem : JobComponentSystem
{
    protected EntityQuery m_Group;


    [BurstCompile]
    struct NeighborPerceptionJob : IJobForEach_BCC<NeighborData, PerceptionData, AgentData>
    {
        [DeallocateOnJobCompletion]
        [NativeDisableParallelForRestriction]
        public NativeArray<AgentData> neighbors;

        public void Execute(DynamicBuffer<NeighborData> b0, ref PerceptionData c1, ref AgentData c2)
        {
            b0.Clear();

            for(int i= 0; i<neighbors.Length; i++)
            {
                if (math.length(c2.Position - neighbors[i].Position) < 10) {
                    b0.Add(new NeighborData { Value = neighbors[i] });
                }
            }

            c1.Clear();
        }
    }

    protected override void OnCreate()
    {
        base.OnCreate();
        m_Group = GetEntityQuery(typeof(AgentData));

    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        //create a hash map of all occupied cells in each particular flock box
        //pass it into the job
        //use data on agent to decide which flock to pull from


        NeighborPerceptionJob job = new NeighborPerceptionJob
        {
            neighbors = m_Group.ToComponentDataArray<AgentData>(Allocator.TempJob)
        };

        return job.Schedule(this, inputDeps);
    }
}
