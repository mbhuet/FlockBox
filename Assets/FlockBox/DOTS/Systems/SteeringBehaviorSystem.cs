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


        protected abstract JobHandle DoPerception();
        protected abstract JobHandle DoSteering();
    }
}