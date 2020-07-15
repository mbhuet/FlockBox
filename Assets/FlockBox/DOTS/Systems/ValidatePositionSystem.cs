using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


namespace CloudFine.FlockBox.DOTS
{
    [UpdateInGroup(typeof(MovementSystemGroup))]
    [UpdateAfter(typeof(VelocitySystem))]
    public class ValidatePositionSystem : JobComponentSystem
    {
        [BurstCompile]
        struct ValidatePositionJob : IJobForEach<Translation, AgentData, BoundaryData>
        {
            public void Execute(ref Translation c0, ref AgentData c1, ref BoundaryData c2)
            {
                c2.ValidatePosition(ref c1.Position);
                c0.Value = c1.Position;
            }
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            ValidatePositionJob job = new ValidatePositionJob
            {
            };
            return job.Schedule(this, inputDeps);
        }
    }
}