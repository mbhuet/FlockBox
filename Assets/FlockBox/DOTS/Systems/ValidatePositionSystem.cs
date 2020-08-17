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
    public class ValidatePositionSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var validationJob = Entities.ForEach((ref AgentData agent, ref Translation translation, ref BoundaryData boundary) =>
            {
                boundary.ValidatePosition(ref agent.Position);
                translation.Value = agent.Position;
            })
            .ScheduleParallel(Dependency);

            Dependency = validationJob;
        }
    }
}