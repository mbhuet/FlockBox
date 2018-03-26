using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TallVisual : SteeringAgentVisual {

    public Gradient statusSpectrum;
    public GameObject shadowCaster;

    
    public static float StatusToHeight(float status)
    {
        return status / 10f + 2f;
    }

    public override void SetRotation(Quaternion rotation)
    {
        //do nothing
    }

    public override void UpdateForAttribute(string attributeName, float value)
    {
        switch (attributeName)
        {
            case SocialStatusBehavior.statusAttributeName:
                //val is from 0-1
                SetColor(statusSpectrum.Evaluate(value/100f));
                SetSize(new Vector2(1, StatusToHeight(value)) );
                break;
        }
    }

    public override void SetSize(Vector2 size)
    {
        base.SetSize(size);
        shadowCaster.transform.localPosition = Vector3.up * size.y / 2f;
        shadowCaster.transform.localScale = new Vector3(2, size.y, 1);
    }

}
