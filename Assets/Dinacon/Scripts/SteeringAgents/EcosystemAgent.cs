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

    public const string energyAttributeName = "energy";

    public float minimumEnergy = 1; //the baseline energy stored by this agent
    public float energyToReproduce = 2; // the energy this agent must collect to reproduce
    public float energyDecayRate = 1;
    public float lifespan = 10;
    protected float energy
    {
        get
        {
            return (float) GetAttribute(energyAttributeName);
        }
        set
        {
            SetAttribute(energyAttributeName, value);
            visual.SetSize(Vector2.one * value);
        }
    }
    protected float eatTime = 1;
    protected float spawnTime;

    protected bool isDying = false;


    protected void Start()
    {
        base.Start();
        InitStateMachine();
    }

    protected void Update()
    {
        base.Update();
        if (isAlive && !isDying)
        {
            EnergyDecay();
            CheckOldAge();
        }

    }

    protected void InitStateMachine()
    {
        fsm = StateMachine<EcoState>.Initialize(this);
        fsm.ChangeState(EcoState.WANDER);
    }

    protected void EnergyDecay()
    {
        energy -= energyDecayRate * Time.deltaTime;
        if (energy <= 0) fsm.ChangeState(EcoState.DIE);
    }

    public override void Spawn(Vector3 position)
    {
        base.Spawn(position);
        spawnTime = Time.time;
        energy = minimumEnergy;
    }


    protected bool IsNourishedEnoughToReproduce()
    {
        if (!HasAttribute(energyAttributeName)) return false;
        return (float)GetAttribute(energyAttributeName) >= minimumEnergy;
    }

    protected void CheckOldAge()
    {
        if (Time.time - spawnTime >= lifespan)
        {
            fsm.ChangeState(EcoState.DIE);
        }
    }

    protected IEnumerator DIE_Enter()
    {
        visual.Blink(true);
        RemoveFromNeighborhood();
        isDying = true;
        float t = 1;
        while (t > 0)
        {
            t -= Time.deltaTime;
            velocityThrottle = t;
            yield return null;
        }
        visual.Blink(false);
        Kill();
        isDying = false;
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
