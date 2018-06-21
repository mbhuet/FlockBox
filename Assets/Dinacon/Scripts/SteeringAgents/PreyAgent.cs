using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreyAgent : EcosystemAgent {
    public BehaviorSettings forageSettings;
    public BehaviorSettings fleeSettings;

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
        fsm.ChangeState(EcoState.EATEN);

    }

    protected void FORAGE_Enter()
    {
        activeSettings = forageSettings;
    }

    protected void FORAGE_Exit()
    {

    }

    protected void FLEE_Enter()
    {
        activeSettings = fleeSettings;
    }

    protected void FLEE_Exit()
    {

    }

}
