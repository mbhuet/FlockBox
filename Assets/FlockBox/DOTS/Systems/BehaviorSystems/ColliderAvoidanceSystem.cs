using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

namespace CloudFine.FlockBox.DOTS
{

    public class ColliderAvoidanceSystem : SteeringBehaviorSystem<ColliderAvoidanceData>
    {
        private float3[] Directions;

        protected override void OnCreate()
        {
            base.OnCreate();

            Vector3[] vector3Directions = ColliderAvoidanceBehavior.Directions;
            Directions = new float3[vector3Directions.Length];
            for(int i=0; i<vector3Directions.Length; i++)
            {
                Directions[i] = (float3)vector3Directions[i];
            }
            
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override JobHandle DoPerception()
        {
            return Dependency;
        }

        protected override JobHandle DoSteering()
        {
            PhysicsWorld physicsWorld = World.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;
            Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();

            //TODO use burst
            Entities
                .WithoutBurst()
                .WithReadOnly(physicsWorld)
                //.WithReadOnly(dirs)
                .ForEach((ref AccelerationData acceleration, ref ColliderAvoidanceData avoidance, in AgentData agent, in SteeringData steering) =>
                {
                    float lookDist = math.length(agent.Velocity) * avoidance.LookAheadSeconds;
                    float castRadius = agent.Radius + avoidance.Clearance;
                    CollisionFilter filter = new CollisionFilter()
                    {
                        BelongsTo = (uint)avoidance.LayerMask,
                        CollidesWith = (uint)avoidance.LayerMask,
                    };

                    //TODO use LocalToWorld to get world position
                    float3 worldPosition = agent.Position;
                    float3 worldForward = agent.Forward;

                    RaycastInput input = new RaycastInput()
                    {
                        Start = worldPosition,
                        End = worldPosition + worldForward * lookDist,
                        Filter = filter
                    };


                    //Something was hit, find best avoidance direction
                    //TODO sphere cast instead of ray
                    if (physicsWorld.CastRay(input, out hit))
                    {
                        UnityEngine.Debug.DrawLine(input.Start, input.End, Color.magenta);
                        
                        float hitDist = math.length(hit.Position - worldPosition);
                        if (math.all(avoidance.LastClearWorldDirection == float3.zero))
                        {
                            avoidance.LastClearWorldDirection = worldForward;
                        }

                        float3 clearWorldDirection = avoidance.LastClearWorldDirection;

                        float3 up = new float3(0, 1, 0);
                        quaternion rot = quaternion.LookRotation(worldForward, up);

                        for (int i = 0; i < Directions.Length; i++)
                        {
                            float3 dir = math.mul(rot, Directions[i]);
                            input.End = worldPosition + dir * lookDist;
                            UnityEngine.Debug.DrawLine(input.Start, input.End, Color.white * .1f);

                            if (!physicsWorld.CastRay(input, out hit))
                            {
                                UnityEngine.Debug.DrawLine(input.Start, input.End, Color.cyan);

                                clearWorldDirection = dir;
                                break;
                            }
                        }


                        avoidance.LastClearWorldDirection = clearWorldDirection;
                        float smooth = (1f - (hitDist / lookDist));

                        //TODO world to local
                        float3 clearLocalDirection = clearWorldDirection;
                        //acceleration.Value += steering.GetSteerVector(clearLocalDirection, agent.Velocity) * avoidance.Weight * smooth;
                        
                    }

                    else
                    {

                        avoidance.LastClearWorldDirection = worldForward;
                    }

                }
                //TODO schedule
                ).Run();// ScheduleParallel(Dependency);
            return Dependency;
        }
    }

    public struct ColliderAvoidanceData : IComponentData
    {
        public bool Active;
        public float Weight;
        public float LookAheadSeconds;
        public float Clearance;
        public Int32 LayerMask;
        public float3 LastClearWorldDirection;
    }
}