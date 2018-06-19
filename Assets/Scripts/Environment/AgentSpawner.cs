using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentSpawner : MonoBehaviour {
    public SteeringAgent boidPrefab;
    public int numStartSpawns;

    // Use this for initialization
    void Start() {
        Spawn(numStartSpawns);
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(0))
        {
            Spawn(1, Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
        
    }

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
            GameObject.Instantiate<SteeringAgent>(boidPrefab, pos, Quaternion.identity);
        }
    }

    
}
