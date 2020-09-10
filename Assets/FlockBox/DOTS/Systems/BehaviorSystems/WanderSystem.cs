using System.Xml;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace CloudFine.FlockBox.DOTS
{
    public class WanderSystem : SteeringBehaviorSystem<WanderData>
    {
        protected override JobHandle DoSteering()
        {
            double time = Time.ElapsedTime;
            return Entities
                .ForEach((ref Acceleration acceleration, in AgentData agent, in WanderData behavior, in SteeringData steering) =>
                {
                    acceleration.Value += behavior.CalculateSteering(agent, steering, (float)time);

                }).ScheduleParallel(Dependency);
        }

    }

    public struct WanderData : IComponentData, ISteeringBehaviorComponentData
    {
        public float Weight;
        public float Scope;
        public float Intensity;

        public float3 CalculateSteering(AgentData mine, SteeringData steering, DynamicBuffer<NeighborData> neighbors)
        {
            return float3.zero;
        }

        /// <summary>
        /// This overload of GetSteering requires time instead of a DynamicBuffer<NeighborData>
        /// </summary>
        /// <param name="mine"></param>
        /// <param name="steering"></param>
        /// <returns></returns>
        public float3 CalculateSteering(AgentData mine, SteeringData steering, float time)
        {
            float UniqueID = mine.UniqueID *.001f;
            UniqueID *= UniqueID;

            float3 dir = new float3(
                noise.cnoise(new float2((time * Intensity), UniqueID) - .5f) * Scope,
                noise.cnoise(new float2((time * Intensity) + UniqueID, UniqueID) - .5f) * Scope,
                noise.cnoise(new float2((time * Intensity) + UniqueID * 2, UniqueID) - .5f) * Scope
                );
            return math.mul(quaternion.Euler(dir), mine.Forward)
                    * steering.MaxForce
                    * Weight;
        }


        public PerceptionData AddPerceptionRequirements(AgentData mine, PerceptionData perception)
        {
            return perception;
        }
    }
}
