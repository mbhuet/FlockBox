using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neighborhood
{
    Dictionary<string, List<Agent>> sortedAgents;

    List<Agent> allAgents;

    public Vector2 neighborhoodCenter {
    get { return (Vector2)Camera.main.transform.position + m_neighborhoodCenter; }
    }
    private Vector2 m_neighborhoodCenter;
    public Neighborhood()
    {
        allAgents = new List<Agent>();
        sortedAgents = new Dictionary<string, List<Agent>>();
    }
    public Neighborhood(Vector2 pos) : this()
    {
        m_neighborhoodCenter = pos;
    }
    public void ClearAgents() { sortedAgents.Clear(); allAgents.Clear(); }
    public bool IsOccupied() { return allAgents.Count > 0; }

    public void AddAgent(Agent occupant)
    {
        string tag = occupant.gameObject.tag;
        List<Agent> agentsOut;
        if (sortedAgents.TryGetValue(tag, out agentsOut))
        {
            agentsOut.Add(occupant);
        }
        else
        {
            List<Agent> newAgents = new List<Agent>();
            newAgents.Add(occupant);
            sortedAgents.Add(tag, newAgents);
        }
        allAgents.Add(occupant);
    }
    public void RemoveAgent(Agent agent)
    {
        string tag = agent.gameObject.tag;
        List<Agent> agentsOut;
        if (sortedAgents.TryGetValue(tag, out agentsOut))
        {
            agentsOut.Remove(agent);
        }
        allAgents.Remove(agent);
    }
    public Dictionary<string, List<Agent>> GetAgents()
    {
        return sortedAgents;
    }
    public List<Agent> GetAgents(string tag)
    {
        List<Agent> agentsOut;
        if (sortedAgents.TryGetValue(tag, out agentsOut))
        {
            return agentsOut;
        }
        else
        {
            return new List<Agent>();
        }
    }

    
}

[System.Serializable]
public struct SurroundingsInfo
{
    public SurroundingsInfo(
        LinkedList<AgentWrapped> allAgents,
        Dictionary<string, LinkedList<AgentWrapped>> sortedAgents)
        { this.allAgents = allAgents; this.sortedAgents = sortedAgents;}
    public LinkedList<AgentWrapped> allAgents;
    public Dictionary<string, LinkedList<AgentWrapped>> sortedAgents;
}

[System.Serializable]
public struct Coordinates
{
    public Coordinates(int r, int c) { row = r; col = c; }
    public int col, row;
}