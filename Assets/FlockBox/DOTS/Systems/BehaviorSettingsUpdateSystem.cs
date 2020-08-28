using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CloudFine.FlockBox.DOTS {

    [UpdateInGroup(typeof(PerceptionSystemGroup))]
    public class BehaviorSettingsUpdateSystem : SystemBase
    {
        protected EntityQuery m_Query;
        private List<BehaviorSettings> toUpdate = new List<BehaviorSettings>();

        protected override void OnCreate()
        {
            m_Query = GetEntityQuery(ComponentType.ReadOnly<BehaviorSettingsData>());

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

                    }).ScheduleParallel(Dependency);
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

            m_Query.SetSharedComponentFilter(new BehaviorSettingsData { Settings = settings });
            NativeArray<Entity> entities = m_Query.ToEntityArray(Allocator.TempJob);
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
