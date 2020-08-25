using CloudFine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace CloudFine.FlockBox.DOTS {

    [UpdateInGroup(typeof(PerceptionSystemGroup))]
    public class BehaviorSettingsUpdateSystem : JobComponentSystem
    {
        protected EntityQuery m_Query;
        protected override void OnCreate()
        {
            m_Query = GetEntityQuery(typeof(BehaviorSettingsData));

            BehaviorSettings.OnSteeringValuesModified += OnSettingsChanged;
            BehaviorSettings.OnBehaviorAdded += OnBehaviorAdded;
            BehaviorSettings.OnBehaviorValuesModified += OnBehaviorModified;
            BehaviorSettings.OnBehaviorRemoved += OnBehaviorRemoved;
        }

        protected override void OnDestroy()
        {
            BehaviorSettings.OnSteeringValuesModified -= OnSettingsChanged;
            BehaviorSettings.OnBehaviorAdded -= OnBehaviorAdded;
            BehaviorSettings.OnBehaviorValuesModified -= OnBehaviorModified;
            BehaviorSettings.OnBehaviorRemoved -= OnBehaviorRemoved;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return inputDeps;
        }


        private void OnSettingsChanged(BehaviorSettings changed)
        {
            SteeringData steerData = changed.ConvertToComponentData();
            m_Query.SetSharedComponentFilter(new BehaviorSettingsData { Settings = changed });
            NativeArray<Entity> entities = m_Query.ToEntityArray(Allocator.TempJob);
            foreach (Entity entity in entities)
            {
                EntityManager.SetComponentData(entity, steerData);
            }
            entities.Dispose();
        }

        private void OnBehaviorAdded(BehaviorSettings settings, SteeringBehavior add)
        {
            IConvertToComponentData convert = add as IConvertToComponentData;
            if (convert == null) return;

            m_Query.SetSharedComponentFilter(new BehaviorSettingsData { Settings = settings });
            NativeArray<Entity> entities = m_Query.ToEntityArray(Allocator.TempJob);
            foreach (Entity entity in entities)
            {
                convert.AddEntityData(entity, EntityManager);
            }
            entities.Dispose();
        }

        private void OnBehaviorModified(BehaviorSettings settings, SteeringBehavior mod)
        {
            IConvertToComponentData convert = mod as IConvertToComponentData;
            if (convert == null) return;

            m_Query.SetSharedComponentFilter(new BehaviorSettingsData { Settings = settings });
            NativeArray<Entity> entities = m_Query.ToEntityArray(Allocator.TempJob);
            foreach (Entity entity in entities)
            {

                convert.SetEntityData(entity, EntityManager);
                //convert.EntityCommandBufferSet(entity, ecb);
            }
            entities.Dispose();
        }

        private void OnBehaviorRemoved(BehaviorSettings settings, SteeringBehavior rem)
        {
            IConvertToComponentData convert = rem as IConvertToComponentData;
            if (convert == null) return;

            m_Query.SetSharedComponentFilter(new BehaviorSettingsData { Settings = settings });
            NativeArray<Entity> entities = m_Query.ToEntityArray(Allocator.TempJob);
            foreach (Entity entity in entities)
            {
                convert.RemoveEntityData(entity, EntityManager);
            }
            entities.Dispose();
        }

    }
}
