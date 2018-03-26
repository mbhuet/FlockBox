using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neighborhood
{
    LinkedList<SteeringAgent> neighbors;
    LinkedList<Obstacle> obstacles;
    Dictionary<string, LinkedList<Target>> targets;


    public Vector2 neighborhoodCenter {
    get { return (Vector2)Camera.main.transform.position + m_neighborhoodCenter; }
    }
    private Vector2 m_neighborhoodCenter;
    public Neighborhood()
    {
        neighbors = new LinkedList<SteeringAgent>();
        obstacles = new LinkedList<Obstacle>();
        targets = new Dictionary<string, LinkedList<Target>>();
    }
    public Neighborhood(Vector2 pos) : this()
    {
        m_neighborhoodCenter = pos;
    }
    public void AddNeighbor(SteeringAgent occupant) { neighbors.AddLast(occupant); }
    public void RemoveNeighbor(SteeringAgent neighbor) { neighbors.Remove(neighbor); }
    public void ClearNeighbors() { neighbors.Clear(); }
    public bool IsOccupied() { return neighbors.First != null && obstacles.First != null; }
    public LinkedList<SteeringAgent> GetNeighbors() { return neighbors; }
    public void AddObstacle(Obstacle obs) { obstacles.AddLast(obs); }
    public LinkedList<Obstacle> GetObstacles() { return obstacles; }

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
    public SurroundingsInfo(LinkedList<SteeringAgentWrapped> boids, LinkedList<ObstacleWrapped> obs, Dictionary<string, LinkedList<TargetWrapped>> targs) { neighbors = boids; obstacles = obs; targets = targs;}
    public LinkedList<SteeringAgentWrapped> neighbors;
    public LinkedList<ObstacleWrapped> obstacles;
    public Dictionary<string, LinkedList<TargetWrapped>> targets;
}

public struct Coordinates
{
    public Coordinates(int r, int c) { row = r; col = c; }
    public int col, row;
}