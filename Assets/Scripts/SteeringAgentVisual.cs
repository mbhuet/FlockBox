using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringAgentVisual : MonoBehaviour {

    public SpriteRenderer sprite;

    public virtual void SetRotation(Quaternion rotation)
    {
        if (sprite != null) sprite.transform.rotation = rotation;
    }

    public void SetColor(Color color)
    {
        sprite.color = color;
    }
}
