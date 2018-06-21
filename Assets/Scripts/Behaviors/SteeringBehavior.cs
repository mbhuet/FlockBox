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

    protected LinkedList<SteeringAgentWrapped> GetFilteredNeighbors(SurroundingsInfo surroundings)
    {
        Dictionary<string, LinkedList<SteeringAgentWrapped>> agentDict = surroundings.neighbors;
        LinkedList<SteeringAgentWrapped> filteredAgents = new LinkedList<SteeringAgentWrapped>();

        LinkedList<SteeringAgentWrapped> agentsOut = new LinkedList<SteeringAgentWrapped>();
        foreach (string tag in filterTags)
        {
            if (agentDict.TryGetValue(tag, out agentsOut))
            {
                foreach (SteeringAgentWrapped agent in agentsOut)
                {
                    filteredAgents.AddLast(agent);
                }

            }
        }
        return filteredAgents;
    }

    protected LinkedList<ZoneWrapped> GetFilteredZones(SurroundingsInfo surroundings)
    {
        Dictionary<string, LinkedList<ZoneWrapped>> zoneDict = surroundings.zones;
        LinkedList<ZoneWrapped> filteredZones = new LinkedList<ZoneWrapped>();

        LinkedList<ZoneWrapped> zonesOut = new LinkedList<ZoneWrapped>();
        foreach (string tag in filterTags)
        {
            if (zoneDict.TryGetValue(tag, out zonesOut))
            {
                foreach (ZoneWrapped avoidZone in zonesOut)
                {
                    filteredZones.AddLast(avoidZone);
                }

            }
        }
        return filteredZones;
    }

    protected LinkedList<TargetWrapped> GetFilteredTargets(SurroundingsInfo surroundings)
    {
        Dictionary<string, LinkedList<TargetWrapped>> targetDict = surroundings.targets;
        LinkedList<TargetWrapped> filteredTargets = new LinkedList<TargetWrapped>();

        LinkedList<TargetWrapped> targetsOut = new LinkedList<TargetWrapped>();
        foreach (string tag in filterTags)
        {
            if (targetDict.TryGetValue(tag, out targetsOut))
            {
                foreach (TargetWrapped target in targetsOut)
                {
                    filteredTargets.AddLast(target);
                }

            }
        }
        return filteredTargets;
    }
    
}
