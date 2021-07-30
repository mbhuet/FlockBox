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
    public class AgentPositionSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var validationJob = Entities.WithNone<SteeringData>().ForEach((ref AgentData agent, in LocalToWorld ltw, in FlockMatrixData flock) =>
            {
                agent.Position = math.transform(flock.WorldToFlockMatrix, ltw.Position);
                agent.Forward = math.transform(flock.WorldToFlockMatrix, ltw.Forward);
                //agent.Position = translation.Value;
            })
            .ScheduleParallel(Dependency);

            Dependency = validationJob;
        }
    }
}
#endif
