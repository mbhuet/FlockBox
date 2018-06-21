using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neighborhood
{
    Dictionary<string, LinkedList<SteeringAgent>> neighbors;
    Dictionary<string, LinkedList<Zone>> zones;
    Dictionary<string, LinkedList<Target>> targets;


    public Vector2 neighborhoodCenter {
    get { return (Vector2)Camera.main.transform.position + m_neighborhoodCenter; }
    }
    private Vector2 m_neighborhoodCenter;
    public Neighborhood()
    {
        neighbors = new Dictionary<string, LinkedList<SteeringAgent>>();
        zones = new Dictionary<string, LinkedList<Zone>>();
        targets = new Dictionary<string, LinkedList<Target>>();
    }
    public Neighborhood(Vector2 pos) : this()
    {
        m_neighborhoodCenter = pos;
    }
    public void ClearNeighbors() { neighbors.Clear(); }
    public bool IsOccupied() { return neighbors.Count > 0; }

    public void AddNeighbor(SteeringAgent occupant)
    {
        string tag = occupant.gameObject.tag;
        LinkedList<SteeringAgent> agentsOut;
        if (neighbors.TryGetValue(tag, out agentsOut))
        {
            agentsOut.AddLast(occupant);
        }
        else
        {
            LinkedList<SteeringAgent> newAgents = new LinkedList<SteeringAgent>();
            newAgents.AddLast(occupant);
            neighbors.Add(tag, newAgents);
        }
    }
    public void RemoveNeighbor(SteeringAgent neighbor)
    {
        string tag = neighbor.gameObject.tag;
        LinkedList<SteeringAgent> agentsOut;
        if (neighbors.TryGetValue(tag, out agentsOut))
        {
            agentsOut.Remove(neighbor);
        }
    }
    public Dictionary<string, LinkedList<SteeringAgent>> GetNeighbors()
    {
        return neighbors;
    }
    public LinkedList<SteeringAgent> GetNeighbors(string tag)
    {
        LinkedList<SteeringAgent> agentsOut;
        if (neighbors.TryGetValue(tag, out agentsOut))
        {
            return agentsOut;
        }
        else
        {
            return new LinkedList<SteeringAgent>();
        }
    }

    public void AddZone(Zone zone)
    {
        string tag = zone.gameObject.tag;
        LinkedList<Zone> zonesOut;
        if (zones.TryGetValue(tag, out zonesOut))
        {
            zonesOut.AddLast(zone);
        }
        else
        {
            LinkedList<Zone> newZones = new LinkedList<Zone>();
            newZones.AddLast(zone);
            zones.Add(tag, newZones);
        }
    }
    public void RemoveZone(Zone zone)
    {
        string tag = zone.gameObject.tag;
        LinkedList<Zone> zonesOut;
        if (zones.TryGetValue(tag, out zonesOut))
        {
            zonesOut.Remove(zone);
        }
    }
    public Dictionary<string, LinkedList<Zone>> GetZones()
    {
        return zones;
    }
    public LinkedList<Zone> GetZones(string tag)
    {
        LinkedList<Zone> zonesOut;
        if (zones.TryGetValue(tag, out zonesOut))
        {
            return zonesOut;
        }
        else
        {
            return new LinkedList<Zone>();
        }
    }

    public void AddTarget(Target target)
    {
        string tag = target.gameObject.tag;
        LinkedList<Target> targetsOut;
        if (targets.TryGetValue(tag, out targetsOut)){
            targetsOut.AddLast(target);
        }
        else
        {
            LinkedList<Target> targs = new LinkedList<Target>();
            targs.AddLast(target);
            targets.Add(tag, targs);
        }
    }
    public void RemoveTarget(Target target)
    {
        string tag = target.gameObject.tag;
        LinkedList<Target> targetsOut;
        if (targets.TryGetValue(tag, out targetsOut))
        {
            targetsOut.Remove(target);
        }
    }
    public Dictionary<string, LinkedList<Target>> GetTargets()
    {
        return targets;
    }
    public LinkedList<Target> GetTargets(string tag) {
        LinkedList<Target> targetsOut;
        if(targets.TryGetValue(tag, out targetsOut)){
            return targetsOut;
        }
        else
        {
            return new LinkedList<Target>();
        }
    }
}

public struct SurroundingsInfo
{
    public SurroundingsInfo(
        LinkedList<SteeringAgentWrapped> allAgents,
        Dictionary<string, LinkedList<SteeringAgentWrapped>> agents, 
        Dictionary<string, LinkedList<ZoneWrapped>> zns, 
        Dictionary<string, LinkedList<TargetWrapped>> targs)
        { allNeighbors = allAgents; neighbors = agents; zones = zns; targets = targs;}
    public LinkedList<SteeringAgentWrapped> allNeighbors;
    public Dictionary<string, LinkedList<SteeringAgentWrapped>> neighbors;
    public Dictionary<string, LinkedList<ZoneWrapped>> zones;
    public Dictionary<string, LinkedList<TargetWrapped>> targets;
}

public struct Coordinates
{
    public Coordinates(int r, int c) { row = r; col = c; }
    public int col, row;
}