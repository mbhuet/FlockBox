using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vexe.Runtime.Types;

[System.Serializable]
public class Agent : BaseBehaviour {

    public Vector3 position { get; protected set; }
    protected Coordinates lastNeighborhood = new Coordinates(0, 0);

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}




    protected bool borders()
    {
        bool mustWrap = false;
        Vector3 wrappedPosition = position;
        if (position.x < NeighborhoodCoordinator.min.x) { wrappedPosition.x = NeighborhoodCoordinator.max.x + (position.x - NeighborhoodCoordinator.min.x); mustWrap = true; }
        if (position.y < NeighborhoodCoordinator.min.y) { wrappedPosition.y = NeighborhoodCoordinator.max.y + (position.y - NeighborhoodCoordinator.min.y); mustWrap = true; }
        if (position.x > NeighborhoodCoordinator.max.x) { wrappedPosition.x = NeighborhoodCoordinator.min.x + (position.x - NeighborhoodCoordinator.max.x); mustWrap = true; }
        if (position.y > NeighborhoodCoordinator.max.y) { wrappedPosition.y = NeighborhoodCoordinator.min.y + (position.y - NeighborhoodCoordinator.max.y); mustWrap = true; }
        if (mustWrap) position = wrappedPosition;
        return mustWrap;
    }
}
