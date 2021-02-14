using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

namespace CloudFine.FlockBox.DOTS
{
    public class ColliderAvoidanceSystem : SteeringBehaviorSystem<ColliderAvoidanceData>
    {

        [BurstCompile]
        public struct RaycastJob : IJob
        {
            public RaycastInput RaycastInput;
            public NativeList<RaycastHit> RaycastHits;
            public bool CollectAllHits;
            [ReadOnly] public PhysicsWorld World;

            public void Execute()
            {
                if (CollectAllHits)
                {
                    World.CastRay(RaycastInput, ref RaycastHits);
                }
                else if (World.CastRay(RaycastInput, out RaycastHit hit))
                {
                    RaycastHits.Add(hit);
                }
            }
        }

        protected override JobHandle DoPerception()
        {
            return Dependency;
        }

        protected override JobHandle DoSteering()
        {
            PhysicsWorld physicsWorld = World.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;

            return Entities
                .WithReadOnly(physicsWorld)
                .ForEach((ref AccelerationData acceleration, in AgentData agent, in SteeringData steering, in ColliderAvoidanceData avoidance) =>
                {
                    float lookDist = math.length(agent.Velocity) * avoidance.LookAheadSeconds;
                    CollisionFilter filter = new CollisionFilter()
                    {
                        CollidesWith = (uint)avoidance.LayerMask
                    };
                    //TODO use LocalToWorld to get world position
                    physicsWorld.SphereCast(agent.Position, agent.Radius + avoidance.Clearance, agent.Forward, lookDist, filter);
                }
                ).ScheduleParallel(Dependency);
        }
    }

    public struct ColliderAvoidanceData : IComponentData
    {
        public bool Active;
        public float Weight;
        public float LookAheadSeconds;
        public float Clearance;
        public Int32 LayerMask;
    }
}