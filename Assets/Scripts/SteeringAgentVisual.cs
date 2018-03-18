using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vexe.Runtime.Types;

public class SteeringAgentVisual : MonoBehaviour {

    public SpriteRenderer sprite;

    public virtual void SetRotation(Quaternion rotation)
    {
        if (sprite != null) sprite.transform.rotation = rotation;
    }

    public virtual void SetSize(Vector2 size)
    {
        if (sprite != null) sprite.size = size;
    }

    public void SetColor(Color color)
    {
        sprite.color = color;
    }

    public virtual void UpdateForAttribute(string attributeName, float value)
    {

    }
}
