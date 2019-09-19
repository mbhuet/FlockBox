using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Each SteeringBehavior will be instantiated ONCE
//That instance will be used by all SteeringAgents

//[DefineCategory("Debug", 3f, "drawVectorLine", "vectorColor")]
[System.Serializable]
public abstract class SteeringBehavior : ScriptableObject{
    public delegate void BehaviorEvent();
    public BehaviorEvent OnActiveStatusChange;

    public bool isActive;

    public float weight = 1;
    public float effectiveRadius = 10;
    public string[] filterTags;
    public bool drawVectorLine;
    public Color vectorColor;

    public abstract Vector3 GetSteeringBehaviorVector(SteeringAgent mine, SurroundingsInfo surroundings);


    private void InvokeActiveChangedEvent(bool active)
    {
        if (OnActiveStatusChange != null) OnActiveStatusChange();
    }

    public static LinkedList<AgentWrapped> GetFilteredAgents(SurroundingsInfo surroundings, params string[] filterTags)
    {
        Dictionary<string, LinkedList<AgentWrapped>> agentDict = surroundings.sortedAgents;
        if(filterTags.Length == 0)
        {
            return surroundings.allAgents;
        }
        LinkedList<AgentWrapped> filteredAgents = new LinkedList<AgentWrapped>();

        LinkedList<AgentWrapped> agentsOut = new LinkedList<AgentWrapped>();
        foreach (string tag in filterTags)
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

    
    
}
