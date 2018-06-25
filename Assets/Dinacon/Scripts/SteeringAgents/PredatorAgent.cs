using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorAgent : EcosystemAgent {
    public BehaviorSettings huntSettings;
    public BehaviorSettings wanderSettings;

    protected float huntInterval = 3;

    protected static List<PredatorAgent> predatorCache;

    protected override void CreateOffspring()
    {
        PredatorAgent offspring = GetPredator();
        offspring.Spawn(position);
    }

    public override void CatchAgent(SteeringAgent other)
    {
        base.CatchAgent(other);
        fsm.ChangeState(EcoState.EAT);
    }


    protected PredatorAgent GetPredator()
    {
        if (predatorCache == null) predatorCache = new List<PredatorAgent>();
        if (predatorCache.Count > 0)
        {
            PredatorAgent predator = predatorCache[0];
            predatorCache.RemoveAt(0);
            predator.gameObject.SetActive(true);
            return predator;
        }
        else return GameObject.Instantiate(this) as PredatorAgent;
    }

    protected override void CacheSelf()
    {
        if (predatorCache == null) predatorCache = new List<PredatorAgent>();

        predatorCache.Add(this);
        gameObject.SetActive(false);

    }

    IEnumerator WANDER_Enter()
    {
        activeSettings = wanderSettings;
        yield return new WaitForSeconds(huntInterval);
        fsm.ChangeState(EcoState.HUNT);
    }

    protected void HUNT_Enter()
    {
        activeSettings = huntSettings;
    }
    protected void HUNT_Update()
    {
        if (IsNourishedEnoughToReproduce())
        {
            fsm.ChangeState(EcoState.REPRODUCE);
        }
    }
}
