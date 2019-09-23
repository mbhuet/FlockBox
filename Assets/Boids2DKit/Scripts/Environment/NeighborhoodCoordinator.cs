using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Should not be a static class because I want to be able to define boundaries without editing the script
//Class will manage itself if there are no instances by creating an instance
//

public struct AgentWrapped
{
    public AgentWrapped(Agent agent, Vector3 wrappedPosition) { this.agent = agent; this.wrappedPosition = wrappedPosition; }
    public Agent agent;
    public Vector3 wrappedPosition;
}


public class NeighborhoodCoordinator : MonoBehaviour {
    private static NeighborhoodCoordinator Instance;
    private static Neighborhood[,] neighborhoods;

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

        neighborhoods = new Neighborhood[rows, cols];
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector2 neighborhoodPos = neighborhoodsCenter_static + new Vector2(((c - cols / 2f) + .5f) * neighborhood_size, ((r - rows / 2f) + .5f) * neighborhood_size);
                neighborhoods[r, c] = new Neighborhood(neighborhoodPos);
            }
        }

        size = maxCorner - minCorner;
        neighborhoodsInitialized = true;
    }

    public static void AddAgent(Agent agent, Coordinates coords)
    {
        if (!neighborhoodsInitialized) InitializeNeighborhoods();
        if (ValidCoordinates(coords))neighborhoods[coords.row, coords.col].AddAgent(agent);
    }

    public static void RemoveAgent(Agent agent, Coordinates coords)
    {
        if (!neighborhoodsInitialized) InitializeNeighborhoods();
        if (ValidCoordinates(coords)) neighborhoods[coords.row, coords.col].RemoveAgent(agent);
    }


    private static bool ValidCoordinates(Coordinates coords)
    {
        if (!neighborhoodsInitialized) InitializeNeighborhoods();
        return (coords.row < neighborhoodRows_static && coords.col < neighborhoodCols_static && coords.row >= 0 && coords.col >= 0);
    }


    public static Coordinates WorldPosToNeighborhoodCoordinates(Vector2 position)
    {
        if (!neighborhoodsInitialized) InitializeNeighborhoods();
        Coordinates coords = new Coordinates();
        position = WrapPosition(position);
        coords.col = Mathf.FloorToInt((position.x - neighborhoodsCenter_static.x + neighborhoodCols_static / 2f * neighborhoodSize_static) / neighborhoodSize_static);
        coords.row = Mathf.FloorToInt((position.y - neighborhoodsCenter_static.y + neighborhoodRows_static / 2f * neighborhoodSize_static) / neighborhoodSize_static);
        coords.col = Mathf.Clamp(coords.col, 0, neighborhoodCols_static-1);
        coords.row = Mathf.Clamp(coords.row, 0, neighborhoodRows_static-1);
        return coords;
    }


    public static void GetSurroundings(ref SurroundingsInfo data, Vector2 position, float perceptionDistance)
    {
        if (!neighborhoodsInitialized) InitializeNeighborhoods();
        LinkedList<AgentWrapped> allAgents = new LinkedList<AgentWrapped>();
        Dictionary<string, LinkedList<AgentWrapped>> sortedAgents = new Dictionary<string, LinkedList<AgentWrapped>>();

        VisitNeighborhoodsWithinCircle(position, perceptionDistance, 
            delegate(Coordinates coords)
            {
                Dictionary<string, List<Agent>> sourceAgents = neighborhoods[coords.row, coords.col].GetSortedAgents();
                foreach (string tag in sourceAgents.Keys)
                {
                    List<Agent> agentsOut;
                    if (sourceAgents.TryGetValue(tag, out agentsOut))
                    {
                        foreach (Agent agent in agentsOut)
                        {
                            AgentWrapped wrappedAgent = new AgentWrapped(agent, (agent.Position + (Vector3)wrap_positionOffset));
                            allAgents.AddFirst(wrappedAgent);
                            if (!sortedAgents.ContainsKey(tag)) sortedAgents.Add(tag, new LinkedList<AgentWrapped>());
                            sortedAgents[tag].AddFirst(wrappedAgent);
                        }
                    }
                }
            }
            );

        data.allAgents = allAgents;
        data.sortedAgents = sortedAgents;
    }

    private static Vector2 wrap_positionOffset = Vector2.zero;
    public static void VisitNeighborhoodsWithinCircle(Vector2 center, float radius, Action<Coordinates> visitFunc)
    {
        if (!neighborhoodsInitialized) InitializeNeighborhoods();
        int neighborhoodRadius = 1 + Mathf.FloorToInt((radius) / neighborhoodSize_static);
        Coordinates centerCoords = WorldPosToNeighborhoodCoordinates(center);

        int r_wrap = 0;
        int c_wrap = 0;
        for (int r = centerCoords.row - neighborhoodRadius; r <= centerCoords.row + neighborhoodRadius; r++)
        {
            for (int c = centerCoords.col - neighborhoodRadius; c <= centerCoords.col + neighborhoodRadius; c++)
            {
                r_wrap = r;
                c_wrap = c;
                wrap_positionOffset = Vector3.zero;
                if (r < 0)
                {
                    r_wrap = neighborhoodRows_static + r;
                    wrap_positionOffset += Vector2.down * neighborhoodRows_static * neighborhoodSize_static;
                }
                else if (r >= neighborhoodRows_static)
                {
                    r_wrap = r - neighborhoodRows_static;
                    wrap_positionOffset += Vector2.up * neighborhoodRows_static * neighborhoodSize_static;
                }
                if (c < 0)
                {
                    c_wrap = neighborhoodCols_static + c;
                    wrap_positionOffset += Vector2.left * neighborhoodCols_static * neighborhoodSize_static;
                }
                else if (c >= neighborhoodCols_static)
                {
                    c_wrap = c - neighborhoodCols_static;
                    wrap_positionOffset += Vector2.right * neighborhoodCols_static * neighborhoodSize_static;
                }

                Neighborhood checkNeighborhood = neighborhoods[r_wrap, c_wrap];
                // Find the closest point to the center within the cell
                Vector2 closestPointInNeighborhood = new Vector2
                    (Mathf.Clamp(center.x - wrap_positionOffset.x,
                        checkNeighborhood.neighborhoodCenter.x - neighborhoodSize_static / 2f,
                        checkNeighborhood.neighborhoodCenter.x + neighborhoodSize_static / 2f),
                    Mathf.Clamp(center.y - wrap_positionOffset.y,
                        checkNeighborhood.neighborhoodCenter.y - neighborhoodSize_static / 2f,
                        checkNeighborhood.neighborhoodCenter.y + neighborhoodSize_static / 2f));

                //Debug.DrawLine(closestPointInNeighborhood + wrap_positionOffset, center, Color.yellow);
                if((center - (closestPointInNeighborhood + wrap_positionOffset)).sqrMagnitude < radius * radius)
                {
                    visitFunc(new Coordinates(r_wrap, c_wrap));
                }

            }
        }
    }


    public static List<Coordinates> AddAreaToNeighborhoods(Agent agent)
    {
        if (!neighborhoodsInitialized) InitializeNeighborhoods();
        List<Coordinates> addedCoords = new List<Coordinates>();
        VisitNeighborhoodsWithinCircle(agent.Position, agent.Radius,
            delegate (Coordinates coords)
            {
                addedCoords.Add(coords);
                neighborhoods[coords.row, coords.col].AddAgent(agent);
            }
            );
        return addedCoords;
    }

    public static Vector3 WrapPosition(Vector3 position)
    {
        if (!neighborhoodsInitialized) InitializeNeighborhoods();
        bool mustWrap = false;
        Vector3 wrappedPosition = position;
        if (position.x < NeighborhoodCoordinator.minCorner.x) { wrappedPosition.x = NeighborhoodCoordinator.maxCorner.x + (position.x - NeighborhoodCoordinator.minCorner.x) % (neighborhoodSize_static * neighborhoodCols_static); mustWrap = true; }
        if (position.y < NeighborhoodCoordinator.minCorner.y) { wrappedPosition.y = NeighborhoodCoordinator.maxCorner.y + (position.y - NeighborhoodCoordinator.minCorner.y) % (neighborhoodSize_static * neighborhoodRows_static); mustWrap = true; }
        if (position.x > NeighborhoodCoordinator.maxCorner.x) { wrappedPosition.x = NeighborhoodCoordinator.minCorner.x + (position.x - NeighborhoodCoordinator.maxCorner.x) % (neighborhoodSize_static * neighborhoodCols_static); mustWrap = true; }
        if (position.y > NeighborhoodCoordinator.maxCorner.y) { wrappedPosition.y = NeighborhoodCoordinator.minCorner.y + (position.y - NeighborhoodCoordinator.maxCorner.y) % (neighborhoodSize_static * neighborhoodRows_static); mustWrap = true; }
        if (mustWrap) position = wrappedPosition;
        return wrappedPosition;
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
