using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidSpawner : MonoBehaviour {
    public Boid boidPrefab;
    public int numStartSpawns;

    // Use this for initialization
    void Start() {
        Spawn(numStartSpawns);
    }

    // Update is called once per frame
    void Update() {

    }

    void Spawn(int numBoids)
    {
        Spawn(numBoids, Vector2.zero);
    }

    void Spawn(int numBoids, Vector2 pos)
    {
        for (int i = 0; i < numBoids; i++)
        {
            GameObject.Instantiate<Boid>(boidPrefab, pos, Quaternion.identity);
        }
    }
}
