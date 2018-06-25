using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreyAgent : EcosystemAgent {
    public BehaviorSettings forageSettings;
    public BehaviorSettings fleeSettings;


    protected static List<PreyAgent> preyCache;
    protected static int numPrey = 0;

    protected override void CreateOffspring()
    {
        PreyAgent offspring = GetPrey();
        offspring.Spawn(position);
    }

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
        else if (IsNourishedEnoughToReproduce())
        {
            fsm.ChangeState(EcoState.REPRODUCE);
        }
    }

    protected void FORAGE_Exit()
    {

    }

    protected void FLEE_Enter()
    {
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

    protected PreyAgent GetPrey()
    {
        if (preyCache == null) preyCache = new List<PreyAgent>();
        if (preyCache.Count > 0)
        {
            PreyAgent prey = preyCache[0];
            preyCache.RemoveAt(0);
            prey.gameObject.SetActive(true);
            return prey;
        }
        else return GameObject.Instantiate(this) as PreyAgent;
    }

    protected override void CacheSelf()
    {
        if (preyCache == null) preyCache = new List<PreyAgent>();
        preyCache.Add(this);
        gameObject.SetActive(false);

    }

}
