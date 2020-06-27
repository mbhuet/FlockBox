using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[UpdateInGroup(typeof(SteeringSystemGroup))]
public class SeparationSystem : JobComponentSystem
{
    [BurstCompile]
    struct SeparationJob : IJobForEach_BCC<NeighborData, Acceleration, SeparationData>
    {


        public void Execute(DynamicBuffer<NeighborData> b0, ref Acceleration c1, ref SeparationData c2)
        {
            if (TagMaskUtility.TagInMask(b0[0].Value.Tag, c2.TagMask))
            {

            }
        }
    }

    struct SeparationPerceptionJob : IJobForEach<SeparationData, PerceptionData>
    {
        public void Execute(ref SeparationData c0, ref PerceptionData c1)
        {
            //add perceptions
            c1.perceptionRadius = Mathf.Max(c1.perceptionRadius, c0.Radius);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        SeparationJob job = new SeparationJob
        {

        };
        return job.Schedule(this, inputDeps);
    }
}
