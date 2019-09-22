using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AgentVisual))]
public class Zone : Agent {

    List<Coordinates> occupyingNeighborhoods = new List<Coordinates>();

    protected override void FindNeighborhood()
    {
        List<Coordinates> newNeighborhoods = NeighborhoodCoordinator.AddZoneToNeighborhoods(this);
        foreach(Coordinates old in occupyingNeighborhoods)
        {
            if (!newNeighborhoods.Contains(old))
            {
                RemoveFromNeighborhood(old);
            }
        }
        occupyingNeighborhoods = newNeighborhoods;
    }
}
