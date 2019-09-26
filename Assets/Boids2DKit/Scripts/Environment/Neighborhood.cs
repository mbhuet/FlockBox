using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neighborhood
{
    Dictionary<string, List<Agent>> sortedAgents;

    //List<Agent> allAgents;
    //List<Agent> stationaryAgents;
    private int agentCount;
    public Vector2 neighborhoodCenter {
        get; protected set;
    }
    public Neighborhood()
    {
        //allAgents = new List<Agent>();
        //stationaryAgents = new List<Agent>();
        sortedAgents = new Dictionary<string, List<Agent>>();
    }
    public Neighborhood(Vector2 pos) : this()
    {
        neighborhoodCenter = pos;
    }
    public void ClearAgents() { sortedAgents.Clear(); } //allAgents.Clear(); 
    public bool IsOccupied() { return agentCount>0; }// allAgents.Count > 0; }

    public void AddAgent(Agent occupant)
    {
        if (occupant == null) return;
        string tag = occupant.gameObject.tag;
        List<Agent> agentsOut;
        if (sortedAgents.TryGetValue(tag, out agentsOut))
        {
            if (!agentsOut.Contains(occupant)){
                agentsOut.Add(occupant);
                agentCount++;
            }
        }
        else
        {
            List<Agent> newAgents = new List<Agent>();
            newAgents.Add(occupant);
            sortedAgents.Add(tag, newAgents);
            agentCount++;
        }
        //allAgents.Add(occupant);
    }
    public void RemoveAgent(Agent agent)
    {
        if (agent == null) return;
        string tag = agent.gameObject.tag;
        List<Agent> agentsOut;
        if (sortedAgents.TryGetValue(tag, out agentsOut))
        {
            if (agentsOut.Remove(agent))
            {
                agentCount--;
            }
        }
        //allAgents.Remove(agent);
    }
    public Dictionary<string, List<Agent>> GetSortedAgents()
    {
        return sortedAgents;
    }

    public bool GetAgentsWithTag(string tag, out List<Agent> agentsOut)
    {   
        if (sortedAgents.TryGetValue(tag, out agentsOut))
        {
            return true;
        }
        else
        {
            agentsOut = new List<Agent>();
            return false;
        }
    }
    
}


