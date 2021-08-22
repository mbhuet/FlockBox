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
            var validationJob = Entities.WithNone<SteeringData>().ForEach((ref AgentData agent, in LocalToWorld ltw, in FlockMatrixData flock) =>
            {
                agent.Position = math.transform(flock.WorldToFlockMatrix, ltw.Position);
                agent.Forward = math.transform(flock.WorldToFlockMatrix, ltw.Forward);
                //TODO Agent.Velocity based on position delta
            })
            .ScheduleParallel(Dependency);

            Dependency = validationJob;
        }
    }
}
#endif
