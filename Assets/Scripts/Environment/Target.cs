using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AgentVisual))]
public class Target : Agent {


    protected bool hasSpawned = false;
    private bool isRegistered = false;

    public int targetID { get; protected set; }

    protected Coordinates currentNeighborhood;

    public int maxPursuers = 1;
    protected int numPursuers = 0;
    public bool useZLayering;

    public bool isCaught { get; protected set; }

    
    protected static int targetCount_static = 0;
    protected static Dictionary<int, Target> targetRegistry;


    public delegate void TargetEvent(Target target);
    public TargetEvent OnCaught;
    public TargetEvent OnSpawn;

    protected void Start()
    {
        if(!hasSpawned)Spawn(transform.position);
        

    }


    protected void RegisterNewTarget()
    {
        targetCount_static++;
        targetID = targetCount_static;
        if (targetRegistry == null) targetRegistry = new Dictionary<int, Target>();
        targetRegistry.Add(targetID, this);
        isRegistered = true;
    }


    void AddToNeighborhood()
    {
        currentNeighborhood = NeighborhoodCoordinator.WorldPosToNeighborhoodCoordinates(position);
        NeighborhoodCoordinator.AddTarget(this, currentNeighborhood);
    }

    void RemoveFromNeighborhood()
    {
        NeighborhoodCoordinator.RemoveTarget(this, currentNeighborhood);

    }

    public static void InformOfPursuit(bool isBeingPursued, SteeringAgent agent, int targetID)
    {
        Target targetOut;
        if(targetRegistry.TryGetValue(targetID, out targetOut))
        {
            targetOut.InformOfPursuit(isBeingPursued, agent);
        }
    }

    public void InformOfPursuit(bool isBeingPursued, SteeringAgent agent)
    {
        if (isBeingPursued) numPursuers++;
        else numPursuers--;
        if (numPursuers < 0) numPursuers = 0;
    }

    public virtual bool CanBePursuedBy(SteeringAgent agent)
    {
        int agentTargetID = (int)agent.GetAttribute(SeekBehavior.targetIDAttributeName);
        return !isCaught && (numPursuers < maxPursuers || agentTargetID == targetID);
    }

    public virtual void Spawn(Vector2 position)
    {
        if(!isRegistered) RegisterNewTarget();

        isCaught = false;
        visual.Show();
        borders();
        this.position = position;
        this.transform.position = (useZLayering ? ZLayering.GetZLayeredPosition(position): (Vector3)position);
        AddToNeighborhood();
        numPursuers = 0;
        hasSpawned = true;
        if (OnSpawn != null) OnSpawn.Invoke(this);
    }


    public virtual void CaughtBy(SteeringAgent agent)
    {
        Kill();
        if (OnCaught != null) OnCaught.Invoke(this);
    }

    public virtual void Kill()
    {
        RemoveFromNeighborhood();
        hasSpawned = false;
        isCaught = true;
        visual.Hide();// = false;
        numPursuers = 0;
    }
}
