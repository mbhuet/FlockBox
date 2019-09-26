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

    protected bool WithinEffectiveRadius(SteeringAgent mine, Agent other)
    {
        if (mine == other) return false;
        return (Vector3.SqrMagnitude(mine.Position - other.Position) < effectiveRadius * effectiveRadius);
    }

    public static List<Agent> GetFilteredAgents(SurroundingsInfo surroundings, SteeringBehavior behavior)// params string[] filterTags)
    {
        if(!behavior.useTagFilter)
        {
            return surroundings.allAgents;
        }
        List<Agent> filteredAgents = new List<Agent>();

        foreach(Agent agent in surroundings.allAgents)
        {
            if(Array.IndexOf(behavior.filterTags, agent.tag) >= 0){
                filteredAgents.Add(agent);
            }
        }
        return filteredAgents;

       
    }



}
