using CloudFine;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


[UpdateInGroup(typeof(PerceptionSystemGroup))]
public class BehaviorSettingsUpdateSystem : JobComponentSystem
{
    protected EntityQuery m_Query;
    private BehaviorSettings[] allSettings;
    private EntityCommandBuffer buffer;

    protected override void OnCreate()
    {
        m_Query = GetEntityQuery(typeof(BehaviorSettingsData));
        allSettings = Resources.FindObjectsOfTypeAll<BehaviorSettings>();
 
        foreach(BehaviorSettings settings in allSettings)
        {
            settings.OnChanged += OnSettingsChanged;
            settings.OnBehaviorAdded += OnBehaviorAdded;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return inputDeps;
        //throw new System.NotImplementedException();
    }

    private void OnSettingsChanged(BehaviorSettings changed)
    {
        m_Query.SetFilter(new BehaviorSettingsData { Settings = changed });
        NativeArray<Entity> entities = m_Query.ToEntityArray(Allocator.TempJob);

        //for each entity, create a job that will update data 

        entities.Dispose();
    }

    private void OnBehaviorAdded(BehaviorSettings settings, SteeringBehavior add)
    {
        m_Query.SetFilter(new BehaviorSettingsData { Settings = settings });
        NativeArray<Entity> entities = m_Query.ToEntityArray(Allocator.TempJob);
        foreach(Entity entity in entities)
        {
            add.EntityCommandBufferAdd(entity, buffer);
        }
        entities.Dispose();
    }

    private void OnBehaviorModified(BehaviorSettings settings, SteeringBehavior mod)
    {
        m_Query.SetFilter(new BehaviorSettingsData { Settings = settings });
        NativeArray<Entity> entities = m_Query.ToEntityArray(Allocator.TempJob);
        foreach (Entity entity in entities)
        {
            mod.EntityCommandBufferSet(entity, buffer);
        }
        entities.Dispose();
    }

    private void OnBehaviorRemoved(BehaviorSettings settings, SteeringBehavior rem)
    {
        m_Query.SetFilter(new BehaviorSettingsData { Settings = settings });
        NativeArray<Entity> entities = m_Query.ToEntityArray(Allocator.TempJob);
        foreach (Entity entity in entities)
        {
            rem.EntityCommandBufferRemove(entity, buffer);
        }
        entities.Dispose();
    }


}
