using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreyAgent : SteeringAgent {
    public BehaviorSettings forageSettings;
    public BehaviorSettings fleeSettings;

    public const string nourishAttributeName = "nourishment";
    public float nourishAmount = 1;

    public override void CaughtBy(SteeringAgent other)
    {
        Debug.Log("prey caught");

        base.CaughtBy(other);
        if (other.HasAttribute(nourishAttributeName))
        {
            float last_nourishment = (float)other.GetAttribute(nourishAttributeName);
            other.SetAttribute(nourishAttributeName, last_nourishment + nourishAmount);
        }
        Kill();

    }

}
