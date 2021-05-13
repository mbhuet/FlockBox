#if FLOCKBOX_DOTS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CloudFine.FlockBox.DOTS {

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class BehaviorSettingsUpdateSystem : SystemBase
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
            foreach (Tuple<BehaviorSettings, IConvertToComponentData> changed in toAdd)
            {
                BehaviorSettingsData filterData = new BehaviorSettingsData { Settings = changed.Item1 };
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



            foreach (Tuple<BehaviorSettings, IConvertToComponentData> changed in toRemove)
            {
                BehaviorSettingsData filterData = new BehaviorSettingsData { Settings = changed.Item1 };
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
    }
}
#endif