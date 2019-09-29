using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;


//Each SteeringBehavior will be instantiated ONCE
//That instance will be used by all SteeringAgents

//[DefineCategory("Debug", 3f, "drawVectorLine", "vectorColor")]
[System.Serializable]
public abstract class SteeringBehavior : ScriptableObject{
    public Action<bool> OnActiveStatusChange;

    public bool IsActive => isActive;
    [SerializeField]
    private bool isActive = true;

    public float weight = 1;
    public float effectiveRadius = 10;
    public bool useTagFilter;
    public string[] filterTags = new string[0];
    public bool drawVectorLine;
    public Color vectorColor = Color.white;

    public abstract void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsInfo surroundings);

    protected bool WithinEffectiveRadius(SteeringAgent mine, AgentWrapped other)
    {
        if (mine == other.agent) return false;
        return (Vector3.SqrMagnitude(mine.Position - other.wrappedPosition) < effectiveRadius * effectiveRadius);
    }

    public static List<AgentWrapped> GetFilteredAgents(SurroundingsInfo surroundings, SteeringBehavior behavior)// params string[] filterTags)
    {
        if (!behavior.useTagFilter) return surroundings.allAgents;

        List<AgentWrapped> filtered = new List<AgentWrapped>();
        foreach(AgentWrapped other in surroundings.allAgents)
        {
            if(Array.IndexOf(behavior.filterTags, other.agent.tag) >= 0)
            {
                filtered.Add(other);
            }
        }
        return filtered;
        
    }



}
