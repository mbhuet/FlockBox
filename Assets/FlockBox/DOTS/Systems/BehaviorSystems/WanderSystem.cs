﻿using Drawing;
using System.Xml.Schema;
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
                .ForEach((ref Acceleration acceleration, in AgentData agent, in WanderData behavior, in SteeringData steering) =>
                {
                    var steer = behavior.CalculateSteering(agent, steering, (float)time);
                    acceleration.Value += steer;
                }
                ).ScheduleParallel(Dependency);
        }
    }

    public struct WanderData : IComponentData
    {
        public float Weight;
        public float Scope;
        public float Intensity;


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

            //noise.cnoise() will return a value (-1, 1)
            float3 dir = new float3(
                (noise.cnoise(new float2((time * Intensity), UniqueID))) * Scope * .5f,
                (noise.cnoise(new float2((time * Intensity) + UniqueID, UniqueID))) * Scope * .5f,
                (noise.cnoise(new float2((time * Intensity) + UniqueID * 2, UniqueID))) * Scope * .5f
                );
//            UnityEngine.Debug.Log("id " + UniqueID + " " + dir + "   " + noise.cnoise(new float2((time * Intensity), UniqueID)));
            return math.mul(quaternion.Euler(math.radians(dir)), mine.Forward)
                    * steering.MaxForce
                    * Weight;
        }
    }
}
