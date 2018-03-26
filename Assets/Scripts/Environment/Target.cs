using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour {

    public Vector3 position { get; protected set; }
    public float radius;

    public int targetID { get; protected set; }

    protected Coordinates currentNeighborhood;

    protected SteeringAgent pursuer;
    public SpriteRenderer visual;


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
        Debug.Log("Being pursued " + isBeingPursued);
        if (isBeingPursued) pursuer = agent;
        else pursuer = null;
    }

    public virtual bool CanBePursuedBy(SteeringAgent agent)
    {
        return agent == pursuer || !pursuer;
    }

    public virtual void Catch(SteeringAgent agent)
    {
        InformOfPursuit(false, agent);
        NeighborhoodCoordinator.RemoveTarget(this, currentNeighborhood);
        visual.enabled = false;
    }
}
