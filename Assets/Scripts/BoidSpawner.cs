using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidSpawner : MonoBehaviour {
    public Boid boidPrefab;
    public Obstacle obstaclePrefab;
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
        if (Input.GetMouseButtonDown(1))
        {
            StartCoroutine(GrowObstacle(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
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
            GameObject.Instantiate<Boid>(boidPrefab, pos, Quaternion.identity);
        }
    }

    IEnumerator GrowObstacle(Vector2 center)
    {
        Obstacle newObstacle = GameObject.Instantiate<Obstacle>(obstaclePrefab, center, Quaternion.identity);
        float radius = 0;
        newObstacle.OnBeginGrow();
        while (Input.GetMouseButton(1))
        {
            radius += Time.deltaTime * 10 * Mathf.Max(1, radius *.1f);
            newObstacle.SetRadius(radius);
            yield return null;
        }
        newObstacle.OnEndGrow();
    }
}
