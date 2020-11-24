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
            //The problem is that I either need a commandbuffer in this foreach or a new IJobChunk that can work with a query


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

        //cannot BurstCompile
        
        protected struct ApplySettingsJob : IJobChunk
        {
            [ReadOnly] public ArchetypeChunkSharedComponentType<BehaviorSettingsData> BehaviorSettingsDataType;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;
            public EntityManager em;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BehaviorSettingsData settings = chunk.GetSharedComponentData(BehaviorSettingsDataType, em);
                var chunkEntities = chunk.GetNativeArray(EntityType);
                for (var i = 0; i < chunk.Count; i++)
                {
                }

            }
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
