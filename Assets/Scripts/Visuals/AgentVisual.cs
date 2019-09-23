using System.Collections;
using UnityEngine;


public class AgentVisual : MonoBehaviour {

    public SpriteRenderer sprite;
    public Transform visualRoot;
    protected bool blinking = false;


    public virtual void SetRotation(Quaternion rotation)
    {
        if (sprite != null)
        {
            visualRoot.transform.rotation = rotation;
        }
    }

    public virtual void SetSprite(Sprite newSprite)
    {
        sprite.sprite = newSprite;
    }

    public virtual void SetRootSize(float size)
    {
        visualRoot.localScale = Vector3.one * size;
    }

    public virtual void SetSpriteSize(Vector2 size)
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
        float blinkRate = 100;
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
