using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterLove.StateMachine;

public abstract  class EcosystemAgent : SteeringAgent {
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

    public float startEnergy = 1; //the baseline energy stored by this agent
    public float energyToReproduce = 2; // the energy this agent must collect to reproduce
    public float base_energyDecayRate = 1;
    protected float energy
    {
        get
        {
            if (!HasAttribute(energyAttributeName)) SetAttribute(energyAttributeName, 0f);
            return (float) GetAttribute(energyAttributeName);
        }
        set
        {
            SetAttribute(energyAttributeName, value);
            visual.SetSpriteSize(Vector2.one * value);
        }
    }
    protected float eatTime = 1;
    protected float spawnTime;
    protected float age
    {
        get { return Time.time - spawnTime; }
    }

    protected bool isDying = false;



    protected void Start()
    {
        base.Start();
    }

    protected void Update()
    {
        base.Update();
        if (isAlive && !isDying)
        {
//            Debug.Log(this.name + " alive, not dying, energy is " + energy + " " + fsm.State);
            EnergyDecay();
        }

    }

    protected void InitStateMachine()
    {
        fsm = StateMachine<EcoState>.Initialize(this);
        fsm.ChangeState(EcoState.WANDER);
    }

    protected void EnergyDecay()
    {
        energy -= base_energyDecayRate * age * Time.deltaTime;
        //velocityThrottle = energy / startEnergy;
        visual.SetRootSize(Mathf.Max(1, energy / startEnergy));
        if (energy <= 0)
        {
            fsm.ChangeState(EcoState.DIE);
        }
    }

    public override void Spawn(Vector3 position)
    {
        base.Spawn(position);
        spawnTime = Time.time;
        energy = startEnergy;
        InitStateMachine();
    }

    protected abstract void CreateOffspring();
    protected abstract void CacheSelf();

    public override void Kill()
    {
        base.Kill();
        ShakeOffSeeds();
        CacheSelf();
    }

    protected void ShakeOffSeeds()
    {
        foreach(FloraSeed seed in gameObject.GetComponentsInChildren<FloraSeed>())
        {
            seed.transform.parent = null;
        }
    }


    protected bool IsNourishedEnoughToReproduce()
    {
        return energy >= energyToReproduce;
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

    protected void REPRODUCE_Enter()
    {
        CreateOffspring();
        energy -= startEnergy;
        fsm.ChangeState(EcoState.WANDER);
    }


}
