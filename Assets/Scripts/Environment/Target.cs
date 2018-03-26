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


    protected static int targetCount = 0;
    protected static Dictionary<int, Target> targetRegistry;


    protected void Start()
    {
        position = transform.position;
        AddToNeighborhood();
        targetCount++;
        targetID = targetCount;
        if (targetRegistry == null) targetRegistry = new Dictionary<int, Target>(); 
        targetRegistry.Add(targetID, this);
    }

    void AddToNeighborhood()
    {
        currentNeighborhood = NeighborhoodCoordinator.WorldPosToNeighborhoodCoordinates(position);
        NeighborhoodCoordinator.AddTarget(this, currentNeighborhood);


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
        return !isCaught && numPursuers <maxPursuers;
    }


    public virtual void CaughtBy(SteeringAgent agent)
    {
        isCaught = true;
        InformOfPursuit(false, agent);
        NeighborhoodCoordinator.RemoveTarget(this, currentNeighborhood);
        visual.enabled = false;
        numPursuers = 0;
    }
}
