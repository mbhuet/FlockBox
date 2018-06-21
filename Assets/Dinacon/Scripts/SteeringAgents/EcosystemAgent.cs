using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterLove.StateMachine;

public class EcosystemAgent : SteeringAgent {
    public enum EcoState
    {
        FLEE, //will ignore everything except pursuers
        HUNT, //will ignore everything except quary
        WANDER, //will not respond to nearby food sources
        FORAGE, //will respond to nearby food sources
        EAT,
        EATEN, //being eaten
        REPRODUCE,
        DIE,
    }

    protected StateMachine<EcoState> fsm;

    public const string nourishAttributeName = "nourishment";

    public float nourishmentToReproduce = 1;
    public float lifespan = 10;
    protected float eatTime = 1;
    protected float spawnTime;

    protected void Start()
    {
        base.Start();
        InitStateMachine();
    }
    protected void InitStateMachine()
    {
        fsm = StateMachine<EcoState>.Initialize(this);
        fsm.ChangeState(EcoState.WANDER);
    }

    public override void Spawn(Vector3 position)
    {
        base.Spawn(position);
        spawnTime = Time.time;
    }


    protected bool IsNourishedEnoughToReproduce()
    {
        if (!HasAttribute(nourishAttributeName)) return false;
        return (float)GetAttribute(nourishAttributeName) >= nourishmentToReproduce;
    }

    protected bool IsOldEnoughToDie()
    {
        return Time.time - spawnTime >= lifespan;
    }

    protected IEnumerator DIE_Enter()
    {
        visual.Blink(true);
        float t = 1;
        while (t > 0)
        {
            t += Time.deltaTime;
            velocityThrottle = t;
            yield return null;
        }
        Kill();

    }


    protected IEnumerator EATEN_Enter()
    {
        RemoveFromNeighborhood();
        velocityThrottle = 0;
        visual.Blink(true);
        yield return new WaitForSeconds(eatTime);
        visual.Blink(false);
        Kill();
    }


    protected IEnumerator EAT_Enter()
    {
        velocityThrottle = 0;
        yield return new WaitForSeconds(eatTime);
        fsm.ChangeState(EcoState.WANDER);
    }

    protected void EAT_Exit()
    {
        velocityThrottle = 1;
    }


}
