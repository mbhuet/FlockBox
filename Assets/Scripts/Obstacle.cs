using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour {
    SpriteRenderer sprite;
    public const float forceFieldDistance = 10; //how close can a Boid be before it hits the force field
    public float radius { get; protected set; }
    public Vector3 center { get; protected set; }

	// Use this for initialization
	void Awake () {
        sprite = GetComponent<SpriteRenderer>();
	}


	
	// Update is called once per frame
	void Start () {
        center = transform.position;

	}

    public void OnBeginGrow()
    {
        SetAlpha(.2f);
    }

    public void OnEndGrow()
    {
        SetAlpha(1);
        Boid.AddObstacleToNeighborhoods(this);
    }

    void SetAlpha(float alpha)
    {
        Color col = sprite.color;
        col.a = alpha;
        sprite.color = col;

    }

    public void SetRadius(float radius)
    {
        this.radius = radius;
        transform.localScale = Vector2.one * radius * 2;
    }

}
