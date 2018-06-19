using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneSpawner : MonoBehaviour {
    public Zone obstaclePrefab;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(1))
        {
            StartCoroutine(GrowObstacle(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
        }
    }

    IEnumerator GrowObstacle(Vector2 center)
    {
        Zone newObstacle = GameObject.Instantiate<Zone>(obstaclePrefab, center, Quaternion.identity);
        float radius = 0;
        newObstacle.OnBeginGrow();
        while (Input.GetMouseButton(1))
        {
            radius += Time.deltaTime * 10 * Mathf.Max(1, radius * .1f);
            newObstacle.SetRadius(radius);
            yield return null;
        }
        newObstacle.OnEndGrow();
    }
}
