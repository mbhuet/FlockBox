using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neighborhood
{
    LinkedList<SteeringAgent> neighbors;
    LinkedList<Obstacle> obstacles;
    public Vector2 neighborhoodCenter;
    public Neighborhood()
    {
        neighbors = new LinkedList<SteeringAgent>();
        obstacles = new LinkedList<Obstacle>();
    }
    public Neighborhood(Vector2 pos) : this()
    {
        neighborhoodCenter = pos;
    }
    public void AddNeighbor(SteeringAgent occupant) { neighbors.AddLast(occupant); }
    public void RemoveNeighbor(SteeringAgent neighbor) { neighbors.Remove(neighbor); }
    public void ClearNeighbors() { neighbors.Clear(); }
    public bool IsOccupied() { return neighbors.First != null && obstacles.First != null; }
    public LinkedList<SteeringAgent> GetNeighbors() { return neighbors; }
    public void AddObstacle(Obstacle obs) { obstacles.AddLast(obs); }
    public LinkedList<Obstacle> GetObstacles() { return obstacles; }

}

public struct SurroundingsInfo
{
    public SurroundingsInfo(LinkedList<SteeringAgent> boids, LinkedList<Obstacle> obs) { neighbors = boids; obstacles = obs; }
    public LinkedList<SteeringAgent> neighbors;
    public LinkedList<Obstacle> obstacles;
}

public struct Coordinates
{
    public Coordinates(int r, int c) { row = r; col = c; }
    public int col, row;
}