using CloudFine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;


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
        var job = new UpdateBehaviorSettingsJob
        {

        };
        //jobHandle = job.Schedule(this, inputDeps);
        //ecb.Schedule(this, inputDeps);
        
        /*
        Entities.ForEach((Entity entity, int entityInQueryIndex, ref Lifetime lifetime) =>
        {
            // Track the lifetime of an entity and destroy it once
            // the lifetime reaches zero
            if (lifetime.Value == 0)
            {
                // pass the entityInQueryIndex to the operation so
                // the ECB can play back the commands in the right
                // order
                ecb.DestroyEntity(entityInQueryIndex, entity);
            }
            else
            {
                lifetime.Value -= 1;
            }
        }).Schedule(inputDeps);
        */

        // Make sure that the ECB system knows about our job
        //m_EndSimulationEcbSystem.AddJobHandleForProducer(jobHandle);
        return default;
    }

    [BurstCompile]
    struct UpdateBehaviorSettingsJob : IJob
    {
        public void Execute()
        {
            throw new System.NotImplementedException();
        }
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
        IConvertToComponentData convert = add as IConvertToComponentData;
        if (convert == null) return;

        Debug.Log("behavior add " + settings.ToString() + " " + add.ToString());
        m_Query.SetFilter(new BehaviorSettingsData { Settings = settings });
        NativeArray<Entity> entities = m_Query.ToEntityArray(Allocator.TempJob);
        foreach(Entity entity in entities)
        {
            convert.AddEntityData(entity, EntityManager);
            //convert.EntityCommandBufferAdd(entity, ecb);
        }
        entities.Dispose();
    }

    private void OnBehaviorModified(BehaviorSettings settings, SteeringBehavior mod)
    {
        IConvertToComponentData convert = mod as IConvertToComponentData;
        if (convert == null) return;
        Debug.Log("behavior mod " + settings.ToString() + " " + mod.ToString());

        m_Query.SetFilter(new BehaviorSettingsData { Settings = settings });
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

        Debug.Log("behavior rem " + settings.ToString() + " " + rem.ToString());

        m_Query.SetFilter(new BehaviorSettingsData { Settings = settings });
        NativeArray<Entity> entities = m_Query.ToEntityArray(Allocator.TempJob);
        foreach (Entity entity in entities)
        {
            convert.RemoveEntityData(entity, EntityManager);
        }
        entities.Dispose();
    }


}
