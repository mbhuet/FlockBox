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
    struct AlignmentJob : IJobForEachWithEntity_EBCC<NeighborData, Acceleration, AlignmentData>
    {

        public void Execute(Entity entity, int index, DynamicBuffer<NeighborData> b0, ref Acceleration c1, ref AlignmentData c2)
        {
            if (TagMaskUtility.TagInMask(b0[0].Value.Tag, c2.TagMask)){
                
            }
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
