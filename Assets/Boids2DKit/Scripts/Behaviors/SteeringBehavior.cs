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

    public static LinkedList<AgentWrapped> GetFilteredAgents(SurroundingsInfo surroundings, SteeringBehavior behavior)// params string[] filterTags)
    {
        Dictionary<string, LinkedList<AgentWrapped>> agentDict = surroundings.sortedAgents;
        if(!behavior.useTagFilter)
        {
            return surroundings.allAgents;
        }
        LinkedList<AgentWrapped> filteredAgents = new LinkedList<AgentWrapped>();

        LinkedList<AgentWrapped> agentsOut = new LinkedList<AgentWrapped>();
        foreach (string tag in behavior.filterTags)
        {
            if (agentDict.TryGetValue(tag, out agentsOut))
            {
                foreach (AgentWrapped agent in agentsOut)
                {
                    //Debug.Log(agent.agent.name + " in filtered list");
                    filteredAgents.AddLast(agent);
                }

            }
        }
        return filteredAgents;
    }

    public void AddTag()
    {
        Array.Resize(ref filterTags, filterTags.Length + 1);
    }

    public void RemoveTag(int index)
    {
        string[] newTags = new string[filterTags.Length - 1];
        for (int i = 0; i < filterTags.Length; i++)
        {
            if (i == index) { }
            else if (i > index) { newTags[i - 1] = filterTags[i]; }
            else { newTags[i] = filterTags[i]; }
        }
        filterTags = newTags;
    }



}
