using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[UpdateInGroup(typeof(SteeringSystemGroup))]
public class ContainmentSystem : JobComponentSystem
{
    [BurstCompile]
    struct ContainmentJob : IJobForEach_BCC<NeighborData, Acceleration, AlignmentData>
    {


        public void Execute(DynamicBuffer<NeighborData> b0, ref Acceleration c1, ref AlignmentData c2)
        {
        }
    }

    struct ContainmentPerceptionJob : IJobForEach<AlignmentData, PerceptionData>
    {
        public void Execute(ref AlignmentData c0, ref PerceptionData c1)
        {
            //add perceptions
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    { 
        ContainmentJob job = new ContainmentJob
        {

        };
        return job.Schedule(this, inputDeps);
    }
}
