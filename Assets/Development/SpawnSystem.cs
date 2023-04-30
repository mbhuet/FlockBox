using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct SpawnSystem : ISystem
{
    private bool _done;
    public void OnCreate(ref SystemState state)
    {
        //throw new System.NotImplementedException();
    }

    public void OnDestroy(ref SystemState state)
    {
        //throw new System.NotImplementedException();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (_done) return;
        //throw new System.NotImplementedException();
        var ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        foreach (var data in SystemAPI.Query < SpawnerData > ())
        {
            Entity e = ecb.Instantiate(data.Prefab);
            ecb.AddComponent(e, new DummyData { });
        }
        _done = true;
    }
}
