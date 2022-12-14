#if FLOCKBOX_DOTS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CloudFine.FlockBox.DOTS {

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class BehaviorSettingsUpdateSystem : SystemBase
    {
        private List<BehaviorSettings> toUpdate = new List<BehaviorSettings>();
        private List<Tuple<BehaviorSettings, IConvertToComponentData>> toAdd = new List<Tuple<BehaviorSettings, IConvertToComponentData>>();
        private List<Tuple<BehaviorSettings, IConvertToComponentData>> toRemove = new List<Tuple<BehaviorSettings, IConvertToComponentData>>();

        protected override void OnCreate()
        {
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

        private void OnSettingsChanged(BehaviorSettings changed)
        {
            toUpdate.Add(changed);
        }

        private void OnBehaviorAdded(BehaviorSettings settings, SteeringBehavior add)
        {
            IConvertToComponentData convert = add as IConvertToComponentData;
            if (convert == null) return;
            toAdd.Add(new Tuple<BehaviorSettings, IConvertToComponentData>(settings, convert));
        }

        private void OnBehaviorRemoved(BehaviorSettings settings, SteeringBehavior rem)
        {
            IConvertToComponentData convert = rem as IConvertToComponentData;
            if (convert == null) return;
            toRemove.Add(new Tuple<BehaviorSettings, IConvertToComponentData>(settings, convert));
        }

        protected override void OnUpdate()
        {
            for (int i = 0; i < toAdd.Count; i++)
            {
                var changed = toAdd[i];
                BehaviorSettingsData filterData = new BehaviorSettingsData { SettingsInstanceID = changed.Item1.GetBehaviorSettingsID() };
                Entities
                    .WithSharedComponentFilter(filterData)
                    .WithStructuralChanges()
                    .ForEach((Entity e) =>
                    {
                        changed.Item2.AddEntityData(e, EntityManager);
                    }
                    ).Run();
            }
            toAdd.Clear();



            for(int i=0; i<toRemove.Count; i++)
            {
                var changed = toRemove[i];
                BehaviorSettingsData filterData = new BehaviorSettingsData { SettingsInstanceID = changed.Item1.GetBehaviorSettingsID() };
                Entities
                    .WithSharedComponentFilter(filterData)
                    .WithStructuralChanges()
                    .ForEach((Entity e) =>
                    {
                        changed.Item2.RemoveEntityData(e, EntityManager);
                    }
                    ).Run();
            }
            toRemove.Clear();



            //For each BehaviorSettings that reported a change, make updates on every relevant entity
            foreach (BehaviorSettings changed in toUpdate)
            {
                float maxSpeed = changed.maxSpeed;
                float maxForce = changed.maxForce;

                BehaviorSettingsData filterData = new BehaviorSettingsData { SettingsInstanceID = changed.GetBehaviorSettingsID() };
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
    }
}
#endif