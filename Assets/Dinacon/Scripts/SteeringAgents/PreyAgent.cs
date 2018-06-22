using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreyAgent : EcosystemAgent {
    public BehaviorSettings forageSettings;
    public BehaviorSettings fleeSettings;




    public override void CaughtBy(SteeringAgent other)
    {

        base.CaughtBy(other);
        float last_nourishment = 0;
        if (other.HasAttribute(energyAttributeName))
        {
             last_nourishment = (float)other.GetAttribute(energyAttributeName);
        }
        other.SetAttribute(energyAttributeName, last_nourishment + energy);

        fsm.ChangeState(EcoState.EATEN);

    }

    public override void CatchTarget(Target target)
    {
        base.CatchTarget(target);
        fsm.ChangeState(EcoState.EAT);
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
