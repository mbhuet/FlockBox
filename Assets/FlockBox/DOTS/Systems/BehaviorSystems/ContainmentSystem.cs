using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


namespace CloudFine.FlockBox.DOTS
{
    public class ContainmentSystem : SteeringBehaviorSystem<ContainmentData>
    {
        /// <summary>
        /// Unlike other behaviors, Containment requires access to BoundaryData, and does not need to be aware of neighbors. Here the default steering job is overridden with a custom job.
        /// </summary>
        protected override void DoSteering()
        {
            Dependency = Entities
                .ForEach((ref Acceleration acceleration, in AgentData agent, in ContainmentData behavior, in SteeringData steering, in BoundaryData boundary) =>
                {
                    acceleration.Value += behavior.GetSteering(agent, steering, boundary);

                }).ScheduleParallel(Dependency);
        }

    }

    public struct ContainmentData : IComponentData, ISteeringBehaviorComponentData
    {
        public float Weight;
        public float LookAheadSeconds;

        public float3 GetSteering(AgentData mine, SteeringData steering, DynamicBuffer<NeighborData> neighbors)
        {
            return float3.zero;
        }

        /// <summary>
        /// This overload of GetSteering requires a BoundaryData instead of a DynamicBuffer<NeighborData>
        /// </summary>
        /// <param name="mine"></param>
        /// <param name="steering"></param>
        /// <param name="boundary"></param>
        /// <returns></returns>
        public float3 GetSteering(AgentData mine, SteeringData steering, BoundaryData boundary)
        {
            if (boundary.Wrap) return float3.zero;

            float3 unclampedFuturePosition = mine.Position + mine.Velocity * LookAheadSeconds;
            float3 containedPosition = unclampedFuturePosition;

            float distanceToBorder = float.MaxValue;

            if (boundary.Dimensions.x > 0)
            {
                distanceToBorder = math.min(distanceToBorder, math.min(mine.Position.x, boundary.Dimensions.x - mine.Position.x));
                containedPosition.x = math.clamp(containedPosition.x, boundary.Margin, boundary.Dimensions.x - boundary.Margin);
            }
            else
            {
                containedPosition.x = 0;
            }

            if (boundary.Dimensions.y > 0)
            {
                distanceToBorder = math.min(distanceToBorder, math.min(mine.Position.y, boundary.Dimensions.y - mine.Position.y));
                containedPosition.y = math.clamp(containedPosition.y, boundary.Margin, boundary.Dimensions.y - boundary.Margin);
            }
            else
            {
                containedPosition.y = 0;
            }

            if (boundary.Dimensions.z > 0)
            {
                distanceToBorder = math.min(distanceToBorder, math.min(mine.Position.z, boundary.Dimensions.z - mine.Position.z));
                containedPosition.z = math.clamp(containedPosition.z, boundary.Margin, boundary.Dimensions.z - boundary.Margin);
            }
            else
            {
                containedPosition.z = 0;
            }

            if (math.all(containedPosition == unclampedFuturePosition))
            {
                return float3.zero;
            }

            if (distanceToBorder <= 0) distanceToBorder = .001f;

            return steering.GetSeekVector(containedPosition, mine.Position, mine.Velocity) * (boundary.Margin / distanceToBorder) * Weight;
        }


        public PerceptionData AddPerceptionRequirements(AgentData mine, PerceptionData perception)
        {
            perception.ExpandLookAheadSeconds(LookAheadSeconds);
            return perception;
        }
    }
}
