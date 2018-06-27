using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentSpawner : MonoBehaviour {
    public Agent boidPrefab;
    public int numStartSpawns;

    // Use this for initialization
    void Start() {
        Spawn(numStartSpawns);
    }

    // Update is called once per frame
    

    void Spawn(int numBoids)
    {
        float randomSpawnRadius = Camera.main.orthographicSize / 2f;
        for (int i = 0; i < numBoids; i++)
        {
            Spawn(1, Random.insideUnitCircle * randomSpawnRadius);
        }
    }

    void Spawn(int numBoids, Vector2 pos)
    {
        for (int i = 0; i < numBoids; i++)
        {
            GameObject.Instantiate<Agent>(boidPrefab, pos, Quaternion.identity);
        }
    }

    
}
