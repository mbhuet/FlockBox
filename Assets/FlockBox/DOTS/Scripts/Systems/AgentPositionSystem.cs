#if FLOCKBOX_DOTS
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace CloudFine.FlockBox.DOTS
{
    [UpdateInGroup(typeof(MovementSystemGroup))]
    public class AgentPositionSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float dt = Time.DeltaTime;
            var validationJob = Entities.WithNone<SteeringData>().ForEach((ref AgentData agent, in LocalToWorld ltw, in FlockMatrixData flock) =>
            {
                float3 newPos = math.transform(flock.WorldToFlockMatrix, ltw.Position);
                agent.Velocity = (newPos - agent.Position) / dt;
                agent.Position = newPos;
                agent.Forward = math.transform(flock.WorldToFlockMatrix, ltw.Forward);
            })
            .ScheduleParallel(Dependency);

            Dependency = validationJob;
        }
    }
}
#endif
