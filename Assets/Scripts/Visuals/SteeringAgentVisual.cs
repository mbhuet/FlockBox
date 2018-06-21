using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vexe.Runtime.Types;

public class SteeringAgentVisual : MonoBehaviour {

    public SpriteRenderer sprite;
    protected bool blinking = false;

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

    public virtual void Show()
    {
        sprite.enabled = true;
    }

    public virtual void Hide()
    {
        sprite.enabled = false;
    }

    public void Blink(bool isBlinking)
    {
        blinking = isBlinking;
        if (isBlinking)
        {
            StopCoroutine("BlinkRoutine");
            StartCoroutine("BlinkRoutine");
        }
    }

    private IEnumerator BlinkRoutine()
    {
        float blinkRate = 20;
        bool visible = true;
        bool visible_last = true;
        while (blinking)
        {
            visible = Mathf.Sin(Time.time * blinkRate) > .75f;
            if(visible && !visible_last)
            {
                Show();
            }
            else if(!visible && visible_last)
            {
                Hide();
            }
            yield return null;
            visible_last = visible;
        }
    }
}
