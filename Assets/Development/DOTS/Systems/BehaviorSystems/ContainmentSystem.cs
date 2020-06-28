using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[UpdateInGroup(typeof(SteeringSystemGroup))]
public class ContainmentSystem : JobComponentSystem
{
    [BurstCompile]
    struct ContainmentJob : IJobForEach_BCC<NeighborData, Acceleration, ContainmentData>
    {


        public void Execute(DynamicBuffer<NeighborData> b0, ref Acceleration c1, ref ContainmentData c2)
        {
            if (TagMaskUtility.TagInMask(b0[0].Value.Tag, c2.TagMask))
            {

            }
        }
    }

    struct ContainmentPerceptionJob : IJobForEach<ContainmentData, PerceptionData>
    {
        public void Execute(ref ContainmentData c0, ref PerceptionData c1)
        {
            //add perceptions
            c1.perceptionRadius = Mathf.Max(c1.perceptionRadius, c0.Radius);
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
