using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;


namespace CloudFine.FlockBox.DOTS
{
    [UpdateInGroup(typeof(SteeringSystemGroup))]
    public abstract class SteeringBehaviorSystem<T> : SystemBase where T : struct, IComponentData, ISteeringBehaviorComponentData
    {
        private EntityQuery perceptionQuery;
        private EntityQuery steeringQuery;

        protected override void OnCreate()
        {
            steeringQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadWrite<Acceleration>(),
                    ComponentType.ReadOnly<NeighborData>(),
                    ComponentType.ReadOnly<AgentData>(),
                    ComponentType.ReadOnly<SteeringData>(),
                    ComponentType.ReadOnly<BoundaryData>(),
                    ComponentType.ReadOnly<T>(),
                }
            });

            perceptionQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadWrite<PerceptionData>(),
                    ComponentType.ReadOnly<AgentData>(),
                    ComponentType.ReadOnly<T>(),
                }
            });
        }


        [BurstCompile]
        protected struct SteeringJob : IJobChunk
        {
            public ArchetypeChunkComponentType<Acceleration> AccelerationDataType;
            [ReadOnly] public ArchetypeChunkBufferType<NeighborData> NeighborDataType;
            [ReadOnly] public ArchetypeChunkComponentType<AgentData> AgentDataType;
            [ReadOnly] public ArchetypeChunkComponentType<SteeringData> SteeringDataType;
            [ReadOnly] public ArchetypeChunkComponentType<BoundaryData> BoundaryDataType;
            [ReadOnly] public ArchetypeChunkComponentType<T> BehaviorDataType;


            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var accelerations = chunk.GetNativeArray(AccelerationDataType);
                var agents = chunk.GetNativeArray(AgentDataType);
                var behaviors = chunk.GetNativeArray(BehaviorDataType);
                var boundaries = chunk.GetNativeArray(BoundaryDataType);
                var steerings = chunk.GetNativeArray(SteeringDataType);
                var neighborhood = chunk.GetBufferAccessor(NeighborDataType);

                for (var i = 0; i < chunk.Count; i++)
                {
                    var acceleration = accelerations[i];
                    var agent = agents[i];
                    var behavior = behaviors[i];
                    var boundary = boundaries[i];
                    var steering = steerings[i];
                    var neighbors = neighborhood[i];

                    acceleration.Value += behavior.GetSteering(ref agent, ref steering, ref boundary, neighbors);
                }
            }
        }

        [BurstCompile]
        protected struct PerceptionJob : IJobChunk
        {
            public ArchetypeChunkComponentType<PerceptionData> PerceptionDataType;
            [ReadOnly] public ArchetypeChunkComponentType<AgentData> AgentDataType;
            [ReadOnly] public ArchetypeChunkComponentType<T> BehaviorDataType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var perceptions = chunk.GetNativeArray(PerceptionDataType);
                var agents = chunk.GetNativeArray(AgentDataType);
                var behaviors = chunk.GetNativeArray(BehaviorDataType);

                for (var i = 0; i < chunk.Count; i++)
                {
                    var perception = perceptions[i];
                    var agent = agents[i];
                    var behavior = behaviors[i];

                    behavior.AddPerceptionRequirements(ref agent, ref perception);
                }
            }
        }

        protected override void OnUpdate()
        {
            PerceptionJob perceptJob = new PerceptionJob
            {
                PerceptionDataType = GetArchetypeChunkComponentType<PerceptionData>(false),
                BehaviorDataType = GetArchetypeChunkComponentType<T>(true),
                AgentDataType = GetArchetypeChunkComponentType<AgentData>(true),
            };
            Dependency = perceptJob.Schedule(perceptionQuery, Dependency);

            SteeringJob job = new SteeringJob
            {
                AccelerationDataType = GetArchetypeChunkComponentType<Acceleration>(false),
                NeighborDataType = GetArchetypeChunkBufferType<NeighborData>(true),
                AgentDataType = GetArchetypeChunkComponentType<AgentData>(true),
                SteeringDataType = GetArchetypeChunkComponentType<SteeringData>(true),
                BoundaryDataType = GetArchetypeChunkComponentType<BoundaryData>(true),
                BehaviorDataType = GetArchetypeChunkComponentType<T>(true),
            };
            Dependency = job.Schedule(steeringQuery, Dependency);
        }
    }
}