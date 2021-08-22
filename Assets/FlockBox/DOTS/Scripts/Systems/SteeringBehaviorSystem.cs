#if FLOCKBOX_DOTS
using System;
using System.Collections.Generic;
using Unity.Burst;
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
        private List<Tuple<BehaviorSettings, SteeringBehavior>> toUpdate = new List<Tuple<BehaviorSettings, SteeringBehavior>>();


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
            public ComponentTypeHandle<T> BehaviorDataType;
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
            //this helps behaviors respond to changes made in the Inspector
            foreach (Tuple<BehaviorSettings, SteeringBehavior> tuple in toUpdate)
            {
                //make sure the SteeringBehavior can be converted to component data that corresponds to this behavior
                IConvertToSteeringBehaviorComponentData<T> convert = tuple.Item2 as IConvertToSteeringBehaviorComponentData<T>;
                if (convert == null) continue;

                //query for all entities that use the changed BehaviorSettings
                BehaviorSettingsData data = new BehaviorSettingsData { Settings = tuple.Item1 };
                updateQuery.SetSharedComponentFilter(data);

                T temp = convert.Convert();
                UpdateDataJob updateJob = new UpdateDataJob
                {
                    BehaviorDataType = GetComponentTypeHandle<T>(false),
                    template = temp
                };
                Dependency = updateJob.ScheduleParallel(updateQuery, Dependency);
            }

            toUpdate.Clear();
        }

        /// <summary>
        /// Responsible for updating PerceptionData with the perception needs of this behavior
        /// </summary>
        /// <returns></returns>
        protected abstract JobHandle DoPerception();
        /// <summary>
        /// Responsible for applying steering force to AccelerationData
        /// </summary>
        /// <returns></returns>
        protected abstract JobHandle DoSteering();
    }
}
#endif