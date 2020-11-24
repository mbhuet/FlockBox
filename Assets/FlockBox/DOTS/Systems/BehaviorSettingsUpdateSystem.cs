using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CloudFine.FlockBox.DOTS {

    [UpdateInGroup(typeof(PerceptionSystemGroup))]
    public class BehaviorSettingsUpdateSystem : SystemBase
    {
        protected EntityQuery settingsChangeQuery;
        private List<BehaviorSettings> toUpdate = new List<BehaviorSettings>();

        protected override void OnCreate()
        {
            settingsChangeQuery = GetEntityQuery(ComponentType.ReadOnly<BehaviorSettingsData>());
            settingsChangeQuery.AddChangedVersionFilter(ComponentType.ReadOnly<BehaviorSettingsData>());

            BehaviorSettings.OnSteeringValuesModified += OnSettingsChanged;
            BehaviorSettings.OnBehaviorAdded += OnBehaviorAdded;
            BehaviorSettings.OnBehaviorRemoved += OnBehaviorRemoved;
        }

        protected override void OnDestroy()
        {
            BehaviorSettings.OnSteeringValuesModified -= OnSettingsChanged;
            BehaviorSettings.OnBehaviorAdded -= OnBehaviorAdded;
            BehaviorSettings.OnBehaviorRemoved -= OnBehaviorRemoved;
        }

        protected override void OnUpdate()
        {
            //TODO this is a huge bottleneck
            
            
            Entities
                .WithChangeFilter<BehaviorSettingsData>()
                .WithStructuralChanges()
                .ForEach((Entity e, in BehaviorSettingsData settings) =>
                {
                    settings.Settings.ApplyToEntity(e, EntityManager);
                }
                ).Run();
            

            foreach(BehaviorSettings changed in toUpdate)
            {
                float maxSpeed = changed.maxSpeed;
                float maxForce = changed.maxForce;

                BehaviorSettingsData filterData = new BehaviorSettingsData { Settings = changed };
                Dependency = Entities
                    .WithSharedComponentFilter(filterData)
                    .ForEach((ref SteeringData data) =>
                    {
                        data.MaxForce = maxForce;
                        data.MaxSpeed = maxSpeed;
                    }
                    ).ScheduleParallel(Dependency);
            }
            toUpdate.Clear();
        }

        private void OnSettingsChanged(BehaviorSettings changed)
        {
            toUpdate.Add(changed);
        }

        private void OnBehaviorAdded(BehaviorSettings settings, SteeringBehavior add)
        {
            IConvertToComponentData convert = add as IConvertToComponentData;
            if (convert == null) return;

            settingsChangeQuery.SetSharedComponentFilter(new BehaviorSettingsData { Settings = settings });
            NativeArray<Entity> entities = settingsChangeQuery.ToEntityArray(Allocator.TempJob);
            foreach (Entity entity in entities)
            {
                convert.AddEntityData(entity, EntityManager);
            }
            entities.Dispose();
        }



        private void OnBehaviorRemoved(BehaviorSettings settings, SteeringBehavior rem)
        {
            IConvertToComponentData convert = rem as IConvertToComponentData;
            if (convert == null) return;

            settingsChangeQuery.SetSharedComponentFilter(new BehaviorSettingsData { Settings = settings });
            NativeArray<Entity> entities = settingsChangeQuery.ToEntityArray(Allocator.TempJob);
            foreach (Entity entity in entities)
            {
                convert.RemoveEntityData(entity, EntityManager);
            }
            entities.Dispose();
        }

    }
}
