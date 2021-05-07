using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace CloudFine.FlockBox.DOTS
{
    public class WanderSystem : SteeringBehaviorSystem<WanderData>
    {

        protected override JobHandle DoPerception()
        {
            return Dependency;
        }

        protected override JobHandle DoSteering()
        {
            double time = Time.ElapsedTime;
            return Entities
                .ForEach((ref AccelerationData acceleration, in AgentData agent, in WanderData behavior, in SteeringData steering, in BoundaryData boundary) =>
                {
                    var steer = behavior.CalculateSteering(agent, steering, (float)time, boundary);
                    acceleration.Value += steer;
                }
                ).ScheduleParallel(Dependency);
        }
    }

    public struct WanderData : IComponentData
    {
        public bool Active;
        public float Weight;
        public float Scope;
        public float Intensity;

        public float3 CalculateSteering(AgentData mine, SteeringData steering, float time, BoundaryData boundary)
        {
            if (!Active) return float3.zero;

            float UniqueID = mine.UniqueID *.001f;
            UniqueID *= UniqueID;

            float3 dir = new float3(
                (noise.cnoise(new float2((time * Intensity), UniqueID))) * Scope * .5f,
                (noise.cnoise(new float2((time * Intensity) + UniqueID, UniqueID))) * Scope * .5f,
                (noise.cnoise(new float2((time * Intensity) + UniqueID * 2, UniqueID))) * Scope * .5f
                );
            return boundary.ValidateDirection(math.mul(quaternion.Euler(math.radians(dir)), mine.Forward))
                    * steering.MaxForce
                    * Weight;
        }
    }
}
