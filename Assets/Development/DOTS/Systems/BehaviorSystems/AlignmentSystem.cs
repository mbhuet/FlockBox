using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[UpdateInGroup(typeof(SteeringSystemGroup))]
public class AlignmentSystem : JobComponentSystem
{
    [BurstCompile]
    struct AlignmentJob : IJobForEach_BCC<NeighborData, Acceleration, AlignmentData>
    {


        public void Execute(DynamicBuffer<NeighborData> b0, ref Acceleration c1, ref AlignmentData c2)
        {
            if (TagMaskUtility.TagInMask(b0[0].Value.Tag, c2.TagMask))
            {

            }
        }
    }

    struct AlignmentPerceptionJob : IJobForEach<AlignmentData, PerceptionData>
    {
        public void Execute(ref AlignmentData c0, ref PerceptionData c1)
        {
            //add perceptions
            c1.perceptionRadius = Mathf.Max(c1.perceptionRadius, c0.Radius);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    { 
        AlignmentJob job = new AlignmentJob
        {

        };
        return job.Schedule(this, inputDeps);
    }
}
