using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour {
    public const float forceFieldDistance = 10; //how close can a Boid be before it hits the force field

    protected SpriteRenderer sprite;
    public float radius = 1;
    public Vector3 center { get; protected set; }

    // Use this for initialization
    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Start()
    {
        center = transform.position;
        SetRadius(radius);
        OnEndGrow();
    }


    public void OnBeginGrow()
    {
        SetAlpha(.2f);
    }

    public void OnEndGrow()
    {
        SetAlpha(1);
        NeighborhoodCoordinator.AddZoneToNeighborhoods(this);
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
