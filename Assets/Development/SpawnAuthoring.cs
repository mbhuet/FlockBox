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
        Entity e = GetEntity(TransformUsageFlags.None);
        AddComponent(e, new SpawnerData
        {
            Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic)
        });
    }
}