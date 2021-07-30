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
            var validationJob = Entities.WithAll<SteeringData>().ForEach((ref LocalToWorld ltw, ref AgentData agent, in FlockMatrixData wtf) =>
            {
                if (!math.all(agent.Velocity == float3.zero))
                {
                    agent.Forward = math.normalize(agent.Velocity);
                }

                ltw.Value = float4x4.TRS(
                    wtf.FlockToWorldPoint(agent.Position),
                    quaternion.LookRotationSafe(wtf.FlockToWorldDirection(agent.Forward), new float3(0, 1, 0)),
                    new float3(1, 1, 1));
            })
            .ScheduleParallel(Dependency);

            Dependency = validationJob;
        }
    }
}
#endif
