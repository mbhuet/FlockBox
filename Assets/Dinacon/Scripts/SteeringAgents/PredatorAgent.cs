using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorAgent : EcosystemAgent {



    protected static List<PredatorAgent> predatorCache;

    protected override void SpawnOffspring()
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

}
