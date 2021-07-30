#if FLOCKBOX_DOTS
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
    [UpdateAfter(typeof(SteeringVelocitySystem))]
    public class SteeringPositionSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var validationJob = Entities.WithAll<SteeringData>().ForEach((ref Translation translation, in AgentData agent) =>
            {
                translation.Value = agent.Position;
            })
            .ScheduleParallel(Dependency);

            Dependency = validationJob;
        }
    }
}
#endif
