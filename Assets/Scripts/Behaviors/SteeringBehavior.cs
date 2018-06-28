using System.Collections;
using System.Collections.Generic;
using Vexe.Runtime.Types;
using UnityEngine;


//Each SteeringBehavior will be instantiated ONCE
//That instance will be used by all SteeringAgents

//[DefineCategory("Debug", 3f, "drawVectorLine", "vectorColor")]
[System.Serializable]
public abstract class SteeringBehavior{
    public delegate void BehaviorEvent();
    public BehaviorEvent OnActiveStatusChange;

    [Display(0f), OnChanged("InvokeActiveChangedEvent")]
    public bool isActive;

    [Display(1f), fSlider(0, 10f), VisibleWhen("isActive")]
    public float weight = 1;
    [Display(2f), VisibleWhen("isActive")]
    public float effectiveRadius = 10;
    [PerItem, Tags, VisibleWhen("isActive")]
    public string[] filterTags;
    [Display(3f), VisibleWhen("isActive")]
    public bool drawVectorLine;
    [Display(4f), VisibleWhen("isActive", "drawVectorLine")]
    public Color vectorColor;

    public abstract Vector3 GetSteeringBehaviorVector(SteeringAgent mine, SurroundingsInfo surroundings);

    private void InvokeActiveChangedEvent(bool active)
    {
        if (OnActiveStatusChange != null) OnActiveStatusChange();
    }

    protected LinkedList<AgentWrapped> GetFilteredAgents(SurroundingsInfo surroundings)
    {
        Dictionary<string, LinkedList<AgentWrapped>> agentDict = surroundings.sortedAgents;
        LinkedList<AgentWrapped> filteredAgents = new LinkedList<AgentWrapped>();

        LinkedList<AgentWrapped> agentsOut = new LinkedList<AgentWrapped>();
        foreach (string tag in filterTags)
        {
            if (agentDict.TryGetValue(tag, out agentsOut))
            {
                foreach (AgentWrapped agent in agentsOut)
                {
                    filteredAgents.AddLast(agent);
                }

            }
        }
        return filteredAgents;
    }

    
    
}
