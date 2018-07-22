using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterLove.StateMachine;
using Vexe.Runtime.Types;
using UnityEngine.SceneManagement;

public  class FaunaAgent : SteeringAgent {
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

    public BehaviorSettings fleeSettings;
    public BehaviorSettings huntSettings;
    public BehaviorSettings wanderSettings;

    public float startEnergy = 1; //the baseline energy stored by this agent
    public float reproductionCost = 2; // the energy this agent must expend to reproduce

    public int idealPopulation = 5;

    public float reproductionInterval = 10; //how often this agent will attempt to reproduce
    protected bool readyToReproduce = false;

    [vSlider(0, 100)]
    public Vector2 satisfactionRange;

    protected bool isFleeing { get { return (bool)GetAttribute(FleeBehavior.fleeAttributeName); } }
    protected bool isSatisfied { get { return energy >= energyGoal; } }
    protected float energyGoal { get { return (readyToReproduce ? reproductionEnergyThreshold : satisfactionRange.y); } }
    protected float reproductionEnergyThreshold { get { return satisfactionRange.y + reproductionCost; } }

    public TextMesh energyCounter;


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
            //slow down agent based on energy
            energyCounter.text = energy.ToString("0.0");
        }
    }
    public const float eatTime = 1f;
    protected bool isDying = false;

    


    
    
    protected void Update()
    {
        speedThrottle = Mathf.Clamp01(energy / satisfactionRange.x);

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
        energy = startEnergy;// Random.Range(satisfactionRange.x, satisfactionRange.y);
        InitStateMachine();
        StartCoroutine(ReproductionCountdown());
    }
    

    protected void BirthOffspring()
    {
        energy -= reproductionCost;
        SpawnOffspring();
        readyToReproduce = false;
        StartCoroutine(ReproductionCountdown());
    }
    protected virtual void SpawnOffspring()
    {
        Agent offspring = GetInstance();
        offspring.Spawn(position);
    }


    public override void Kill()
    {
        ShakeOffSeeds();
        base.Kill();
    }

    public override void CatchAgent(Agent other)
    {
        base.CatchAgent(other);
        fsm.ChangeState(EcoState.EAT);
    }

    public override void CaughtBy(Agent other)
    {

        base.CaughtBy(other);
        float last_nourishment = 0;
        if (other.HasAttribute(energyAttributeName))
        {
            last_nourishment = (float)other.GetAttribute(energyAttributeName);
        }
        other.SetAttribute(energyAttributeName, last_nourishment + energy);

        fsm.ChangeState(EcoState.EATEN);
//        Debug.Log(this.name + " Caught By "+ other.name);
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

        else if (!isSatisfied)
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
        if (isFleeing)
        {
            fsm.ChangeState(EcoState.FLEE);
        }
        else if (isSatisfied)
        {
            fsm.ChangeState(EcoState.WANDER);
        }
    } 


    protected IEnumerator DIE_Enter()
    {
        visual.Blink(true);
        RemoveFromLastNeighborhood();
        isDying = true;
        float t = 1;
        while (t > 0)
        {
            t -= Time.deltaTime;
            //velocityThrottle = t;
            yield return null;
        }
        visual.Blink(false);
        isDying = false;

        Kill();
    }


    protected IEnumerator EATEN_Enter()
    {
        isDying = true;
        isAlive = false;

        RemoveFromLastNeighborhood();
        energy = 0;
        visual.Blink(true);
        yield return new WaitForSeconds(eatTime);
        visual.Blink(false);

        isDying = false;

        Kill();
    }


    protected IEnumerator EAT_Enter()
    {
        LockPosition(true);

        for(float t =0; t<eatTime; t += Time.deltaTime)
        {
            if (isFleeing)
            {
                fsm.ChangeState(EcoState.FLEE);
                yield break;
            }
            yield return null;
        }
        if(isAlive && !isCaught) 
            fsm.ChangeState(EcoState.WANDER);
    }

    protected void EAT_Exit()
    {
        LockPosition(false);

    }

    protected void FLEE_Enter()
    {
        activeSettings = fleeSettings;
    }

    protected void FLEE_Update()
    {
        if (!(bool)GetAttribute(FleeBehavior.fleeAttributeName))
        {
            fsm.ChangeState(EcoState.WANDER);
        }
    }





}
