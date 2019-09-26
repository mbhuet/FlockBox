using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Should not be a static class because I want to be able to define boundaries without editing the script
//Class will manage itself if there are no instances by creating an instance
//

    /*
public struct AgentWrapped
{
    public AgentWrapped(Agent agent, Vector3 wrappedPosition) { this.agent = agent; this.wrappedPosition = wrappedPosition; }
    public Agent agent;
    public Vector3 wrappedPosition;
}
*/

public struct SurroundingsInfo
{
    public List<Agent> allAgents;
}


public class NeighborhoodCoordinator : MonoBehaviour {
    private static NeighborhoodCoordinator Instance;
    //private static Neighborhood[,] neighborhoods;
    private static SpatialHash<Agent> neighborhoodHash;

    private static bool neighborhoodsInitialized = false;
    private const float defaultNeighborhoodSize = 10;

    public bool displayGizmos;
    public int neighborhoodCols = 10;
    public int neighborhoodRows = 10;
    public float neighborhoodSize = 10;

    private static int neighborhoodCols_static;
    private static int neighborhoodRows_static;
    private static float neighborhoodSize_static;
    private static Vector2 neighborhoodsCenter_static;
    private Transform trackingTarget;

    public static Vector2 maxCorner { get; private set; }
    public static Vector2 minCorner { get; private set; }
    public static Vector2 size { get; private set; }


    private static List<Neighborhood> toDraw = new List<Neighborhood>();
    public static bool HasMoved
    {
        get; private set;
    }

    void Awake()
    {
        //If there is an instance of NeighborhoodCoordinator in the scene, it will assign itself 
        if (Instance != null && Instance != this) GameObject.Destroy(this);
        else
        {
            Instance = this;
            InitializeNeighborhoods(neighborhoodRows, neighborhoodCols, neighborhoodSize, this.transform);
        }
    }

    void Start()
    {
        trackingTarget = this.transform;
    }

    private void Update()
    {
        if (trackingTarget!= null && trackingTarget.hasChanged)
        {
            HasMoved = true;
            UpdateStaticValues(trackingTarget);
            trackingTarget.hasChanged = false;
        }
        else
        {
            HasMoved = false;
        }
    }


    /*
private void OnDrawGizmos()
{
    if(displayGizmos)DrawNeighborHoods();
}


void DrawNeighborHoods()
{
    if (neighborhoods == null) return;
    for (int r = 0; r < neighborhoods.GetLength(0); r++)
    {
        for (int c = 0; c < neighborhoods.GetLength(1); c++)
        {
            Gizmos.color = Color.red * .1f;
            Vector2 neighborhoodPos = neighborhoods[r, c].neighborhoodCenter;
            if (toDraw.Contains(neighborhoods[r, c]))
            {
                Gizmos.DrawCube(neighborhoodPos, Vector2.one * neighborhoodSize);
            }
            if (neighborhoods[r, c].IsOccupied())
            {
                Gizmos.DrawCube(neighborhoodPos, Vector2.one * neighborhoodSize);
            }
            Gizmos.DrawWireCube(neighborhoodPos, Vector2.one * neighborhoodSize);
        }
    }
    toDraw.Clear();
}
*/


    private static void InitializeNeighborhoods()
    {

        Vector2 viewExtents = new Vector2(Camera.main.orthographicSize * Camera.main.aspect, Camera.main.orthographicSize);
        float wrappingPadding = 1;
        
        int neighborhoodCols = Mathf.FloorToInt((viewExtents.x + wrappingPadding) * 2 / defaultNeighborhoodSize) + 1;
        int neighborhoodRows = Mathf.FloorToInt((viewExtents.y + wrappingPadding) * 2 / defaultNeighborhoodSize) + 1;

        InitializeNeighborhoods(neighborhoodRows, neighborhoodCols, defaultNeighborhoodSize, Camera.main.transform);
    }

    private static void UpdateStaticValues(Transform target)
    {
        neighborhoodsCenter_static = target.position;
        maxCorner = neighborhoodsCenter_static + Vector2.right * (neighborhoodCols_static * neighborhoodSize_static) / 2f + Vector2.up * (neighborhoodRows_static * neighborhoodSize_static) / 2f;
        minCorner = neighborhoodsCenter_static + Vector2.left * (neighborhoodCols_static * neighborhoodSize_static) / 2f + Vector2.down * (neighborhoodRows_static * neighborhoodSize_static) / 2f;
    }


    private static void InitializeNeighborhoods(int rows, int cols, float neighborhood_size, Transform centerTarget)
    {
        Debug.Log("NeighborhoodCoordinator Init " + rows + " " + cols + " " + neighborhood_size);

        neighborhoodSize_static = neighborhood_size;
        neighborhoodCols_static = cols;
        neighborhoodRows_static = rows;
        UpdateStaticValues(centerTarget);

        neighborhoodHash = new SpatialHash<Agent>(neighborhoodSize_static);

        size = maxCorner - minCorner;
        neighborhoodsInitialized = true;
    }

    public static void UpdateAgentPosition(Agent agent, ref List<int> buckets)
    {
        if (neighborhoodHash==null) return;
        neighborhoodHash.UpdatePosition(agent.Position, agent.Radius, agent, out buckets);
    }

    public static void RemoveFromAllNeighborhoods(Agent agent)
    {
        if (neighborhoodHash == null) return;
        neighborhoodHash.Remove(agent);
    }

    public static void GetSurroundings(ref SurroundingsInfo data, Vector2 position, float perceptionDistance, ref List<int> buckets)
    {
        if (neighborhoodHash == null) return;
        neighborhoodHash.QueryPosition(position, perceptionDistance, out data.allAgents, out buckets);

    }


    public static void WrapPosition(ref Vector3 position)
    {
        if (!neighborhoodsInitialized) return;
        if (position.x < NeighborhoodCoordinator.minCorner.x) { position.x = NeighborhoodCoordinator.maxCorner.x + (position.x - NeighborhoodCoordinator.minCorner.x) % (neighborhoodSize_static * neighborhoodCols_static);}
        if (position.y < NeighborhoodCoordinator.minCorner.y) { position.y = NeighborhoodCoordinator.maxCorner.y + (position.y - NeighborhoodCoordinator.minCorner.y) % (neighborhoodSize_static * neighborhoodRows_static);}
        if (position.x > NeighborhoodCoordinator.maxCorner.x) { position.x = NeighborhoodCoordinator.minCorner.x + (position.x - NeighborhoodCoordinator.maxCorner.x) % (neighborhoodSize_static * neighborhoodCols_static);}
        if (position.y > NeighborhoodCoordinator.maxCorner.y) { position.y = NeighborhoodCoordinator.minCorner.y + (position.y - NeighborhoodCoordinator.maxCorner.y) % (neighborhoodSize_static * neighborhoodRows_static);}

    }

    public static Vector3 ClosestPositionWithWrap(Vector3 myPosition, Vector3 otherPosition)
    {
        if (!neighborhoodsInitialized) InitializeNeighborhoods();
        if (Mathf.Abs(myPosition.x - otherPosition.x) > NeighborhoodCoordinator.size.x / 2f)
        {
            otherPosition.x += NeighborhoodCoordinator.size.x * (myPosition.x > otherPosition.x ? 1 : -1);
        }
        if (Mathf.Abs(myPosition.y - otherPosition.y) > NeighborhoodCoordinator.size.y / 2f)
        {
            otherPosition.y += NeighborhoodCoordinator.size.y * (myPosition.y > otherPosition.y ? 1 : -1);
        }
        return otherPosition;
    }


    public static Vector3 RandomPosition()
    {
        if (!neighborhoodsInitialized) InitializeNeighborhoods();
        return new Vector3(UnityEngine.Random.Range(minCorner.x, maxCorner.x), UnityEngine.Random.Range(minCorner.y, maxCorner.y));
    }
}
