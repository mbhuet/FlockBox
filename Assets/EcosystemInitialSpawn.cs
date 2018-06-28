using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EcosystemInitialSpawn : MonoBehaviour {

    public FaunaAgent predatorPrefab;
    public FaunaAgent preyPrefab;
    public FloraTarget floraPrefab;

    public int initialPredators = 1;
    public int initialPrey = 10;
    public int initialFlora = 3;


    private void Start()
    {
        InitialSpawn();
    }

    void InitialSpawn()
    {
        float randomSpawnRadius = Camera.main.orthographicSize;
        for (int i = 0; i < initialPredators; i++)
        {
            SpawnAgent(predatorPrefab, Random.insideUnitCircle * randomSpawnRadius);
        }
        for (int i = 0; i < initialPrey; i++)
        {
            SpawnAgent(preyPrefab, Random.insideUnitCircle * randomSpawnRadius);
        }

        for (int i = 0; i < initialFlora; i++)
        {
            SpawnAgent(floraPrefab, Random.insideUnitCircle * randomSpawnRadius);
        }

    }

    void SpawnAgent(Agent prefab, Vector2 pos)
    {
        Agent instance = prefab.GetInstance();
        instance.Spawn(pos);
    }

    
}
