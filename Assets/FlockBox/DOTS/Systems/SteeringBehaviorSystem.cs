using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;


namespace CloudFine.FlockBox.DOTS
{
    /// <summary>
    /// This base System will handle updating data from associated BehaviorSettings
    /// Subclasses will need to implement steering and perception behavior.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [UpdateInGroup(typeof(SteeringSystemGroup))]
    public abstract class SteeringBehaviorSystem<T> : SystemBase where T : struct, IComponentData
    {
        private EntityQuery updateQuery;
        private EntityQuery removeQuery;
        private EntityQuery addQuery;

        EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;

        protected List<Tuple<BehaviorSettings, SteeringBehavior>> toUpdate = new List<Tuple<BehaviorSettings, SteeringBehavior>>();


        protected override void OnCreate()
        {
            updateQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<BehaviorSettingsData>(),
                    ComponentType.ReadWrite<T>(),
                }
            });

            removeQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<BehaviorSettingsData>(),
                    ComponentType.ReadOnly<T>(),
                }
            });
            addQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<BehaviorSettingsData>(),
                },
                None = new ComponentType[]
                {
                    ComponentType.ReadOnly<T>()
                }
    
            });
            removeQuery.SetChangedVersionFilter(typeof(BehaviorSettingsData));
            addQuery.SetChangedVersionFilter(typeof(BehaviorSettingsData));

            m_EndSimulationEcbSystem = World
            .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            //listen for changes to behaviors
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

        //cannot BurstCompile
        protected struct RemoveDataJob : IJobChunk
        {
            [ReadOnly] public ArchetypeChunkSharedComponentType<BehaviorSettingsData> BehaviorSettingsDataType;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;
            public EntityCommandBuffer buffer;
            public EntityManager em;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BehaviorSettingsData settings = chunk.GetSharedComponentData(BehaviorSettingsDataType, em);
                if (!settings.Settings.RequiresComponentData<T>())
                {
                    var entities = chunk.GetNativeArray(EntityType);
                    for (var i = 0; i < chunk.Count; i++)            
                    {
                        buffer.RemoveComponent<T>(entities[i]);
                        //em.RemoveComponent<T>(entities[i]);
                    }
                }
            }
        }

        //cannot BurstCompile
        protected struct AddDataJob : IJobChunk
        {
            [ReadOnly] public ArchetypeChunkSharedComponentType<BehaviorSettingsData> BehaviorSettingsDataType;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;
            public EntityCommandBuffer buffer;
            public EntityManager em;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BehaviorSettingsData settings = chunk.GetSharedComponentData(BehaviorSettingsDataType, em);
                if (settings.Settings.RequiresComponentData<T>())
                {
                    var entities = chunk.GetNativeArray(EntityType);
                    for (var i = 0; i < chunk.Count; i++)
                    {
                        buffer.AddComponent<T>(entities[i]);
                        //em.RemoveComponent<T>(entities[i]);
                    }
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
            var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer();

            //if an Entity's BehaviorSettings have changed, check to make sure this componentData is still needed. Remove if not.
            RemoveDataJob removeJob = new RemoveDataJob
            {
                BehaviorSettingsDataType = GetArchetypeChunkSharedComponentType<BehaviorSettingsData>(),
                EntityType = GetArchetypeChunkEntityType(),
                em = EntityManager,
                buffer = ecb
            };
            var removeHandle = removeJob.ScheduleParallel(removeQuery, Dependency);

            m_EndSimulationEcbSystem.AddJobHandleForProducer(removeHandle);

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


        protected abstract JobHandle DoPerception();
        protected abstract JobHandle DoSteering();
    }
}