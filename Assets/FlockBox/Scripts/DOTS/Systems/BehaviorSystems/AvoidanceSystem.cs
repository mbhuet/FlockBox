using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace CloudFine.FlockBox.DOTS
{
    public class AvoidanceSystem : SteeringBehaviorSystem<AvoidanceData>
    {
        protected override JobHandle DoPerception()
        {
            return Entities
                .ForEach((ref PerceptionData perception, in AvoidanceData avoidance) =>
                {
                    perception.ExpandLookAheadSeconds(avoidance.LookAheadSeconds);
                }
                ).ScheduleParallel(Dependency);
        }

        protected override JobHandle DoSteering()
        {
            return Entities
                .ForEach((DynamicBuffer<NeighborData> neighbors, ref AccelerationData acceleration, in AgentData agent, in SteeringData steering, in AvoidanceData avoidance) =>
                {
                    acceleration.Value += avoidance.CalculateSteering(agent, steering, neighbors);
                }
                ).ScheduleParallel(Dependency);
        }
    }

    public struct AvoidanceData : IComponentData
    {
        public bool Active;
        public float Weight;
        public float LookAheadSeconds;
        public float Clearance;
        public Int32 TagMask;



        //TODO keep track for current target for later retrieval
        public float3 CalculateSteering(AgentData mine, SteeringData steering, DynamicBuffer<NeighborData> neighbors)
        {
            if (!Active) return float3.zero;


            if (neighbors.Length == 0)
            {
                return float3.zero;
            }

            float rayDist = LookAheadSeconds * math.length(mine.Velocity);
            float closestHitDist = rayDist;
            float hitDist = 0;
            float3 hitCenter = float3.zero;

            for (int i = 0; i < neighbors.Length; i++)
            {
                AgentData other = neighbors[i].Value;
                if (other.TagInMask(TagMask))
                {
                    if (other.FindRayIntersection(mine.Position, mine.Forward, rayDist, mine.Radius + Clearance, ref hitDist))
                    {
                        if (hitDist < closestHitDist)
                        {
                            closestHitDist = hitDist;
                            hitCenter = other.Position;
                        }
                    }
                }
            }

            if (closestHitDist >= rayDist)
            {
                return float3.zero;
            }

            //inside obstacle, steer out
            if(closestHitDist == 0)
            {
                return math.normalize(mine.Position - hitCenter) * steering.MaxForce * Weight;
            }

            //use projection to find steering direction perpendicular to current forward and away from obstacle center
            return math.normalize(mine.Position + mine.Forward * math.dot(hitCenter - mine.Position, mine.Forward) - hitCenter) 
                * steering.MaxForce 
                * Weight 
                * (1f - (closestHitDist / rayDist));
        }
    }
}