using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SpawnAuthoring : MonoBehaviour
{
    public GameObject Prefab;

    
}

public struct SpawnerData : IComponentData
{
    public Entity Prefab;
}

public class SpawnerBaker : Baker<SpawnAuthoring>
{
    public override void Bake(SpawnAuthoring authoring)
    {
        AddComponent( new SpawnerData
        {
            Prefab = GetEntity(authoring.Prefab)
        });
    }
}