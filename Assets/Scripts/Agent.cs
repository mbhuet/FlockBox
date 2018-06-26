using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vexe.Runtime.Types;

[System.Serializable]
[RequireComponent(typeof(AgentVisual))]
public class Agent : BaseBehaviour {

    public Vector3 position { get; protected set; }

    public float radius = 1f;

    protected Coordinates lastNeighborhood = new Coordinates(0, 0);

    private AgentVisual m_visual;
    public AgentVisual visual
    {
        get
        {
            if (m_visual == null) m_visual = GetComponent<AgentVisual>();
            return m_visual;
        }
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
