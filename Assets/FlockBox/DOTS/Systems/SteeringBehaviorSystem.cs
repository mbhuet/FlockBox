using System;
using System.Collections.Generic;
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
        private EntityQuery updateQuery;

        protected List<Tuple<BehaviorSettings, SteeringBehavior>> toUpdate = new List<Tuple<BehaviorSettings, SteeringBehavior>>();


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
                },                
            });
            //perceptionQuery.SetChangedVersionFilter(ComponentType.ReadOnly<T>());


            updateQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<BehaviorSettingsData>(),
                    ComponentType.ReadWrite<T>(),
                }
            });


            BehaviorSettings.OnBehaviorValuesModified += OnBehaviorModified;

        }

        protected override void OnDestroy()
        {
            BehaviorSettings.OnBehaviorValuesModified -= OnBehaviorModified;
        }

        private void OnBehaviorModified(BehaviorSettings settings, SteeringBehavior mod)
        {
            toUpdate.Add(new Tuple<BehaviorSettings, SteeringBehavior>(settings, mod));
        }


        [BurstCompile]
        protected struct SteeringJob : IJobChunk
        {
            public ArchetypeChunkComponentType<Acceleration> AccelerationDataType;
            [ReadOnly] public ArchetypeChunkBufferType<NeighborData> NeighborDataType;
            [ReadOnly] public ArchetypeChunkComponentType<AgentData> AgentDataType;
            [ReadOnly] public ArchetypeChunkComponentType<SteeringData> SteeringDataType;
            [ReadOnly] public ArchetypeChunkComponentType<T> BehaviorDataType;


            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var accelerations = chunk.GetNativeArray(AccelerationDataType);
                var agents = chunk.GetNativeArray(AgentDataType);
                var behaviors = chunk.GetNativeArray(BehaviorDataType);
                var steerings = chunk.GetNativeArray(SteeringDataType);
                var neighborhood = chunk.GetBufferAccessor(NeighborDataType);

                for (var i = 0; i < chunk.Count; i++)
                {
                    var agent = agents[i];
                    if (agent.Sleeping) continue;

                    var acceleration = accelerations[i];
                    acceleration.Value += behaviors[i].CalculateSteering(agent, steerings[i], neighborhood[i]);
                    accelerations[i] = acceleration;
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
                    var agent = agents[i];
                    if (agent.Sleeping) continue;

                    perceptions[i] = behaviors[i].AddPerceptionRequirements(agent, perceptions[i]);
                }
            }
        }


        [BurstCompile]
        protected struct UpdateDataJob : IJobChunk
        {
            public ArchetypeChunkComponentType<T> BehaviorDataType;
            public T template;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var behaviors = chunk.GetNativeArray(BehaviorDataType);

                for (var i = 0; i < chunk.Count; i++)
                {
                    behaviors[i] = template;
                }
            }
        }

        protected override void OnUpdate()
        {
            DoBehaviorDataUpdate();

            var perceptionHandle = DoPerception();

            var steeringHandle = DoSteering();

            Dependency = JobHandle.CombineDependencies(steeringHandle, perceptionHandle, Dependency);
        }

        protected void DoBehaviorDataUpdate()
        {
            foreach (Tuple<BehaviorSettings, SteeringBehavior> tuple in toUpdate)
            {
                IConvertToSteeringBehaviorComponentData<T> convert = tuple.Item2 as IConvertToSteeringBehaviorComponentData<T>;
                if (convert == null) continue;

                BehaviorSettingsData data = new BehaviorSettingsData { Settings = tuple.Item1 };
                updateQuery.SetSharedComponentFilter(data);

                T temp = convert.Convert();
                UpdateDataJob updateJob = new UpdateDataJob
                {
                    BehaviorDataType = GetArchetypeChunkComponentType<T>(false),
                    template = temp
                };
                Dependency = updateJob.ScheduleParallel(updateQuery, Dependency);
            }

            toUpdate.Clear();
        }

        protected virtual JobHandle DoPerception()
        {
            PerceptionJob perceptJob = new PerceptionJob
            {
                //write
                PerceptionDataType = GetArchetypeChunkComponentType<PerceptionData>(false),
                //read
                BehaviorDataType = GetArchetypeChunkComponentType<T>(true),
                AgentDataType = GetArchetypeChunkComponentType<AgentData>(true),
            };
            return perceptJob.ScheduleParallel(perceptionQuery, Dependency);
        }

        protected virtual JobHandle DoSteering()
        {
            SteeringJob job = new SteeringJob
            {
                //write
                AccelerationDataType = GetArchetypeChunkComponentType<Acceleration>(false),
                //read
                NeighborDataType = GetArchetypeChunkBufferType<NeighborData>(true),
                AgentDataType = GetArchetypeChunkComponentType<AgentData>(true),
                SteeringDataType = GetArchetypeChunkComponentType<SteeringData>(true),
                BehaviorDataType = GetArchetypeChunkComponentType<T>(true),

            };
            return job.ScheduleParallel(steeringQuery, Dependency);
            
        }
    }
}