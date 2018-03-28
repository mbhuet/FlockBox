using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour {

    public Vector3 position { get; protected set; }
    public float radius;

    public int targetID { get; protected set; }

    protected Coordinates currentNeighborhood;

    public int maxPursuers = 1;
    protected int numPursuers = 0;
    public SpriteRenderer visual;

    public bool isCaught { get; protected set; }


    protected static int targetCount_static = 0;
    protected static Dictionary<int, Target> targetRegistry;


    public delegate void TargetEvent(Target target);
    public TargetEvent OnCaught;
    public TargetEvent OnSpawn;

    protected void Start()
    {
        RegisterNewTarget();
        Spawn(transform.position);
        

    }

    protected void RegisterNewTarget()
    {
        targetCount_static++;
        targetID = targetCount_static;
        if (targetRegistry == null) targetRegistry = new Dictionary<int, Target>();
        targetRegistry.Add(targetID, this);
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
        isCaught = false;
        this.position = position;
        this.transform.position = ZLayering.GetZLayeredPosition(position);
        AddToNeighborhood();
        numPursuers = 0;
        if (OnSpawn != null) OnSpawn.Invoke(this);
    }


    public virtual void CaughtBy(SteeringAgent agent)
    {
        RemoveFromNeighborhood();
        isCaught = true;
        visual.enabled = false;
        numPursuers = 0;
        if (OnCaught != null) OnCaught.Invoke(this);
    }
}
