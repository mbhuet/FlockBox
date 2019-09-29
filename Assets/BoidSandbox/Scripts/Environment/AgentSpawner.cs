using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentSpawner : MonoBehaviour {
    public Agent prefab;
    public int numStartSpawns;

    // Use this for initialization
    void Start() {
        Spawn(numStartSpawns);
    }
    

    void Spawn(int numBoids)
    {
        if (prefab == null)
        {
            Debug.LogWarning("AgentSpawner.prefab is null");
            return;
        }
        float randomSpawnRadius = Camera.main.orthographicSize / 2f;
        for (int i = 0; i < numBoids; i++)
        {
            Spawn(1, NeighborhoodCoordinator.Instance.RandomPosition());
        }
    }

    void Spawn(int numBoids, Vector3 pos)
    {
        if(prefab == null)
        {
            return;
        }
        for (int i = 0; i < numBoids; i++)
        {
            GameObject.Instantiate<Agent>(prefab, pos, Quaternion.identity);
        }
    }

    
}
