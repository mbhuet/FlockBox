using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterLove.StateMachine;
using Vexe.Runtime.Types;

public abstract  class EcosystemAgent : SteeringAgent {
    public enum EcoState
    {
        FLEE, //will ignore everything except pursuers
        HUNT, //will respond to food sources
        WANDER, //will not respond to nearby food sources
        EAT, //is eating
        EATEN, //is being eaten
        DIE,
    }



    protected StateMachine<EcoState> fsm;

    public const string energyAttributeName = "energy";

    public BehaviorSettings huntSettings;
    public BehaviorSettings wanderSettings;

    public float startEnergy = 1; //the baseline energy stored by this agent
    public float reproductionCost = 2; // the energy this agent must expend to reproduce

    public float reproductionInterval = 10; //how often this agent will attempt to reproduce
    protected bool readyToReproduce = false;

    [vSlider(0, 100)]
    public Vector2 satisfactionRange;

    protected bool isSatisfied { get { return energy >= energyGoal; } }
    protected float energyGoal { get { return (readyToReproduce ? reproductionEnergyThreshold : satisfactionRange.y); } }
    protected float reproductionEnergyThreshold { get { return satisfactionRange.y + reproductionCost; } }

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
        visual.SetRootSize(Mathf.Clamp(energy / startEnergy, 1, 2));
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
        StartCoroutine(ReproductionCountdown());
    }
    

    protected void BirthOffspring()
    {
        energy -= reproductionCost;
        SpawnOffspring();
        StartCoroutine(ReproductionCountdown());
    }
    protected abstract void SpawnOffspring();
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

    protected IEnumerator ReproductionCountdown()
    {
        readyToReproduce = false;
        yield return new WaitForSeconds(reproductionInterval);
        readyToReproduce = true;
    }



    protected void WANDER_Enter()
    {
        activeSettings = wanderSettings;
        
    }

    protected void WANDER_Update()
    {
        if (readyToReproduce && energy >= reproductionEnergyThreshold)
        {
            BirthOffspring();
        }
        if (!isSatisfied)
        {
            fsm.ChangeState(EcoState.HUNT);
        }
    }

    protected void HUNT_Enter()
    {
        activeSettings = huntSettings;
    }
    
    protected void HUNT_Update() 
    {
        if (isSatisfied)
        {
            fsm.ChangeState(EcoState.WANDER);
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
        velocityThrottle = 1;

        Kill();
    }


    protected IEnumerator EAT_Enter()
    {
        velocityThrottle = 0;
        yield return new WaitForSeconds(eatTime);
        velocityThrottle = 1;

        fsm.ChangeState(EcoState.WANDER);
    }


    


}
