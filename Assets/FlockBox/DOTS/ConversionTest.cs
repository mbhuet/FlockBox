using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class ConversionTest : MonoBehaviour
{
    public GameObject prefab;
    public int num = 10;

    // Start is called before the first frame update
    void Start()
    {
        EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;

        GameObjectConversionSettings settings = new GameObjectConversionSettings()
        {
            DestinationWorld = World.DefaultGameObjectInjectionWorld
        };

        Entity entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, settings);


        NativeArray<Entity> agents = new NativeArray<Entity>(num, Allocator.Temp);
        manager.Instantiate(entityPrefab, agents);
        
        agents.Dispose();
    }

}
