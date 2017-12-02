﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Should not be a static class because I want to be able to define boundaries without editing the script
//Class will manage itself if there are no instances by creating an instance
//

public struct SteeringAgentWrapped
{
    public SteeringAgentWrapped(SteeringAgent agent, Vector3 wrappedPosition) { this.agent = agent; this.wrappedPosition = wrappedPosition; }
    public SteeringAgent agent;
    public Vector3 wrappedPosition;
}

public struct ObstacleWrapped
{
    public ObstacleWrapped(Obstacle obstacle, Vector3 wrappedCenter) { this.obstacle = obstacle; this.wrappedCenter = wrappedCenter; }
    public Obstacle obstacle;
    public Vector3 wrappedCenter;
}

public class NeighborhoodCoordinator : MonoBehaviour {
    private static NeighborhoodCoordinator Instance;
    private static Neighborhood[,] neighborhoods;

    private static bool neighborhoodsInitialized = false;
    private const float defaultNeighborhoodDimension = 10;


    public int neighborhoodCols;
    public int neighborhoodRows;
    public Vector2 neighborhoodSize;

    private static int neighborhoodCols_static;
    private static int neighborhoodRows_static;
    private static Vector2 neighborhoodSize_static;
    private static Vector2 neighborhoodsCenter_static;

    public static Vector2 max { get; private set; }
    public static Vector2 min { get; private set; }
    public static Vector2 size { get; private set; }


    void Awake()
    {
        //If there is an instance of NeighborhoodCoordinator in the scene, it will assign itself 
        if (Instance != null && Instance != this) GameObject.Destroy(this);
        else Instance = this;
    }

    void Start()
    {
        if (!neighborhoodsInitialized)
            InitializeNeighborhoods(neighborhoodRows, neighborhoodCols, neighborhoodSize, transform.position);
    }

    private void OnDrawGizmos()
    {
        DrawNeighborHoods();
    }

    void DrawNeighborHoods()
    {
        if (neighborhoods == null) return;
        for (int r = 0; r < neighborhoods.GetLength(0); r++)
        {
            for (int c = 0; c < neighborhoods.GetLength(1); c++)
            {
                Gizmos.color = Color.red;
                Vector2 neighborhoodPos = neighborhoods[r, c].neighborhoodCenter;
                if (neighborhoods[r, c].GetNeighbors().First != null)
                {
                    Gizmos.DrawCube(neighborhoodPos, neighborhoodSize);
                }
                else
                {
                    Gizmos.DrawWireCube(neighborhoodPos, neighborhoodSize);
                }

            }
        }
    }


    private static void InitializeNeighborhoods()
    {

        Vector2 viewExtents = new Vector2(Camera.main.orthographicSize * Camera.main.aspect, Camera.main.orthographicSize);
        Vector2 camPos = Camera.main.transform.position;
        float wrappingPadding = 1;

        Vector2 neighborhoodSize = Vector2.one * defaultNeighborhoodDimension;
        int neighborhoodCols = Mathf.FloorToInt((viewExtents.x + wrappingPadding) * 2 / neighborhoodSize.x) + 1;
        int neighborhoodRows = Mathf.FloorToInt((viewExtents.y + wrappingPadding) * 2 / neighborhoodSize.y) + 1;

        InitializeNeighborhoods(neighborhoodRows, neighborhoodCols, neighborhoodSize, camPos);
    }


    private static void InitializeNeighborhoods(int rows, int cols, Vector2 neighborhood_size, Vector2 center)
    {
        Debug.Log("NeighborhoodCoordinator Init " + rows + " " + cols + " " + neighborhood_size);

        neighborhoodSize_static = neighborhood_size;
        neighborhoodCols_static = cols;
        neighborhoodRows_static = rows;
        neighborhoodsCenter_static = center;

        neighborhoods = new Neighborhood[rows, cols];
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector2 neighborhoodPos = center + new Vector2(((c - cols / 2f) + .5f) * neighborhood_size.x, ((r - rows / 2f) + .5f) * neighborhood_size.y);
                neighborhoods[r, c] = new Neighborhood(neighborhoodPos);
            }
        }

        max = center + Vector2.right * (cols * neighborhood_size.x)/ 2f + Vector2.up * (rows * neighborhood_size.y )/ 2f;
        min = center + Vector2.left * (cols * neighborhood_size.x) / 2f + Vector2.down * (rows * neighborhood_size.y) / 2f;
        size = max - min;

        neighborhoodsInitialized = true;
    }

    public static void AddNeighbor(SteeringAgent agent, Coordinates coords)
    {
        if (!neighborhoodsInitialized) InitializeNeighborhoods();
        if (ValidCoordinates(coords))neighborhoods[coords.row, coords.col].AddNeighbor(agent);
    }

    public static void RemoveNeighbor(SteeringAgent agent, Coordinates coords)
    {
        if (!neighborhoodsInitialized) InitializeNeighborhoods();
        if (ValidCoordinates(coords)) neighborhoods[coords.row, coords.col].RemoveNeighbor(agent);
    }

    private static bool ValidCoordinates(Coordinates coords)
    {
        return (coords.row < neighborhoodRows_static && coords.col < neighborhoodCols_static && coords.row >= 0 && coords.col >= 0);
    }


    public static Coordinates WorldPosToNeighborhoodCoordinates(Vector2 position)
    {
        if (!neighborhoodsInitialized) InitializeNeighborhoods();
        Coordinates coords = new Coordinates();
        coords.col = Mathf.FloorToInt((position.x - neighborhoodsCenter_static.x + neighborhoodCols_static / 2f * neighborhoodSize_static.x) / neighborhoodSize_static.x);
        coords.row = Mathf.FloorToInt((position.y - neighborhoodsCenter_static.y + neighborhoodRows_static / 2f * neighborhoodSize_static.y) / neighborhoodSize_static.y);
        coords.col = Mathf.Clamp(coords.col, 0, neighborhoodCols_static-1);
        coords.row = Mathf.Clamp(coords.row, 0, neighborhoodRows_static-1);
        return coords;
    }


    public static SurroundingsInfo GetSurroundings(Coordinates homeNeighborhoodCoords, int neighborhoodRadius)
    {
        if (!neighborhoodsInitialized) InitializeNeighborhoods();

        LinkedList<SteeringAgentWrapped> allNeighbors = new LinkedList<SteeringAgentWrapped>();
        LinkedList<ObstacleWrapped> allObstacles = new LinkedList<ObstacleWrapped>();
        List<Obstacle> obstacles_alreadyAdded = new List<Obstacle>();

        for (int r = homeNeighborhoodCoords.row - neighborhoodRadius; r <= homeNeighborhoodCoords.row + neighborhoodRadius; r++)
        {
            for (int c = homeNeighborhoodCoords.col - neighborhoodRadius; c <= homeNeighborhoodCoords.col + neighborhoodRadius; c++)
            {
                int r_wrap = r;
                int c_wrap = c;

                Vector3 wrap_positionOffset = Vector3.zero;
                if (r < 0)
                {
                    r_wrap = neighborhoodRows_static + r;
                    wrap_positionOffset += Vector3.up * neighborhoodRows_static * neighborhoodSize_static.y;
                }
                else if (r >= neighborhoodRows_static)
                {
                    r_wrap = r - neighborhoodRows_static;
                    wrap_positionOffset += Vector3.down * neighborhoodRows_static * neighborhoodSize_static.y;
                }
                if (c < 0)
                {
                    c_wrap = neighborhoodCols_static + c;
                    wrap_positionOffset += Vector3.right * neighborhoodCols_static * neighborhoodSize_static.x;
                }
                else if (c >= neighborhoodCols_static)
                {
                    c_wrap = c - neighborhoodCols_static;
                    wrap_positionOffset += Vector3.left * neighborhoodCols_static * neighborhoodSize_static.x;
                }

                foreach (SteeringAgent neighbor in neighborhoods[r_wrap, c_wrap].GetNeighbors())
                {
                    allNeighbors.AddLast(new SteeringAgentWrapped(neighbor, neighbor.position + wrap_positionOffset));
                }
                foreach (Obstacle obstacle in neighborhoods[r_wrap, c_wrap].GetObstacles())
                {
                    if (!obstacles_alreadyAdded.Contains(obstacle))
                    {
                        obstacles_alreadyAdded.Add(obstacle);
                        allObstacles.AddLast(new ObstacleWrapped(obstacle, obstacle.center + wrap_positionOffset));
                    }
                }
                
            }
        }
        SurroundingsInfo data = new SurroundingsInfo(allNeighbors, allObstacles);
        return data;
    }

    public static void AddObstacleToNeighborhoods(Obstacle obs)
    {
        Coordinates centerCoords = WorldPosToNeighborhoodCoordinates(obs.center);
        int radius = 1 + Mathf.FloorToInt((obs.radius + Obstacle.forceFieldDistance) / neighborhoodSize_static.x);
        //        Debug.Log((obs.radius + Obstacle.forceFieldDistance) + " " + radius);
        for (int r = centerCoords.row - radius; r <= centerCoords.row + radius; r++)
        {
            for (int c = centerCoords.col - radius; c <= centerCoords.col + radius; c++)
            {
                if (r >= 0 && r < neighborhoodRows_static && c >= 0 && c < neighborhoodCols_static)
                {
                    Neighborhood checkNeighborhood = neighborhoods[r, c];
                    // Find the closest point to the circle within the rectangle
                    float closestX = Mathf.Clamp(obs.center.x,
                        checkNeighborhood.neighborhoodCenter.x - neighborhoodSize_static.x / 2f,
                        checkNeighborhood.neighborhoodCenter.x + neighborhoodSize_static.x / 2f);
                    float closestY = Mathf.Clamp(obs.center.y,
                        checkNeighborhood.neighborhoodCenter.y - neighborhoodSize_static.y / 2f,
                        checkNeighborhood.neighborhoodCenter.y + neighborhoodSize_static.y / 2f);

                    // Calculate the distance between the circle's center and this closest point
                    float distanceX = obs.center.x - closestX;
                    float distanceY = obs.center.y - closestY;

                    // If the distance is less than the circle's radius, an intersection occurs
                    float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
                    if (distanceSquared < ((obs.radius + Obstacle.forceFieldDistance) * (obs.radius + Obstacle.forceFieldDistance)))
                        neighborhoods[r, c].AddObstacle(obs);
                }
            }
        }
    }
}
