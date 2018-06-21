using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreyAgent : EcosystemAgent {
    public BehaviorSettings forageSettings;
    public BehaviorSettings fleeSettings;

    public float nourishAmount = 1;



    public override void CaughtBy(SteeringAgent other)
    {

        base.CaughtBy(other);
        float last_nourishment = 0;
        if (other.HasAttribute(nourishAttributeName))
        {
             last_nourishment = (float)other.GetAttribute(nourishAttributeName);
        }
        other.SetAttribute(nourishAttributeName, last_nourishment + nourishAmount);

        fsm.ChangeState(EcoState.EATEN);

    }

    protected void WANDER_Enter()
    {
        fsm.ChangeState(EcoState.FORAGE);
    }

    protected void FORAGE_Enter()
    {
        activeSettings = forageSettings;
    }

    protected void FORAGE_Update()
    {
        if ((bool)GetAttribute(FleeBehavior.fleeAttributeName))
        {
            fsm.ChangeState(EcoState.FLEE);
        }
        if (IsNourishedEnoughToReproduce())
        {
            fsm.ChangeState(EcoState.REPRODUCE);
        }
    }

    protected void FORAGE_Exit()
    {

    }

    protected void FLEE_Enter()
    {
        Debug.Log("flee enter");
        activeSettings = fleeSettings;
    }

    protected void FLEE_Update()
    {
        if (!(bool)GetAttribute(FleeBehavior.fleeAttributeName))
        {
            fsm.ChangeState(EcoState.FORAGE);
        }
    }

    protected void FLEE_Exit()
    {

    }

}
