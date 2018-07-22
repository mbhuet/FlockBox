using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Should not be a static class because I want to be able to define boundaries without editing the script
//Class will manage itself if there are no instances by creating an instance
//

public struct SurroundingsDefinition
{
    public SurroundingsDefinition(int row, int col, int rad) { neighborhoodCol = col;  neighborhoodRow = row; radius = rad; }
    public int neighborhoodRow;
    public int neighborhoodCol;
    public int radius;
}

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
    private const float defaultNeighborhoodDimension = 10;

    public bool displayGizmos;
    public int neighborhoodCols = 10;
    public int neighborhoodRows = 10;
    public Vector2 neighborhoodSize = Vector2.one * defaultNeighborhoodDimension;

    private static int neighborhoodCols_static;
    private static int neighborhoodRows_static;
    private static Vector2 neighborhoodSize_static;
    private static Vector2 neighborhoodsCenter_static;
    private Transform trackingTarget;

    public static Vector2 maxCorner { get; private set; }
    public static Vector2 minCorner { get; private set; }
    public static Vector2 size { get; private set; }

    public static Dictionary<SurroundingsDefinition, SurroundingsInfo> cachedSurroundings;

    private static List<Neighborhood> toDraw = new List<Neighborhood>();


    void Awake()
    {
        //If there is an instance of NeighborhoodCoordinator in the scene, it will assign itself 
        if (Instance != null && Instance != this) GameObject.Destroy(this);
        else Instance = this;
    }

    void Start()
    {
        trackingTarget = this.transform;
        if (!neighborhoodsInitialized) { 
            InitializeNeighborhoods(neighborhoodRows, neighborhoodCols, neighborhoodSize, this.transform);
    }
    }

    private void Update()
    {
        if (trackingTarget!= null && trackingTarget.hasChanged)
        {
            UpdateStaticValues(trackingTarget);
            WrapStationaryAgentsInEdgeNeighborhoods();
            trackingTarget.hasChanged = false;
        }
    }

    private void LateUpdate()
    {
        if(cachedSurroundings != null) cachedSurroundings.Clear();
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
                Gizmos.color = Color.red;
                Vector2 neighborhoodPos = neighborhoods[r, c].neighborhoodCenter;
                if (toDraw.Contains(neighborhoods[r, c]))
                {
                    //Gizmos.DrawCube(neighborhoodPos, neighborhoodSize);
                }
                if (neighborhoods[r, c].IsOccupied())
                {
                    //Gizmos.DrawCube(neighborhoodPos, neighborhoodSize);
                }


                Gizmos.DrawWireCube(neighborhoodPos, neighborhoodSize);


            }
        }
        toDraw.Clear();
    }


    private static void InitializeNeighborhoods()
    {

        Vector2 viewExtents = new Vector2(Camera.main.orthographicSize * Camera.main.aspect, Camera.main.orthographicSize);
        float wrappingPadding = 1;
        
        Vector2 neighborhoodSize = Vector2.one * defaultNeighborhoodDimension;
        int neighborhoodCols = Mathf.FloorToInt((viewExtents.x + wrappingPadding) * 2 / neighborhoodSize.x) + 1;
        int neighborhoodRows = Mathf.FloorToInt((viewExtents.y + wrappingPadding) * 2 / neighborhoodSize.y) + 1;

        InitializeNeighborhoods(neighborhoodRows, neighborhoodCols, neighborhoodSize, Camera.main.transform);
    }

    private static void UpdateStaticValues(Transform target)
    {
        neighborhoodsCenter_static = target.position;
        maxCorner = neighborhoodsCenter_static + Vector2.right * (neighborhoodCols_static * neighborhoodSize_static.x) / 2f + Vector2.up * (neighborhoodRows_static * neighborhoodSize_static.y) / 2f;
        minCorner = neighborhoodsCenter_static + Vector2.left * (neighborhoodCols_static * neighborhoodSize_static.x) / 2f + Vector2.down * (neighborhoodRows_static * neighborhoodSize_static.y) / 2f;
    }


    private static void InitializeNeighborhoods(int rows, int cols, Vector2 neighborhood_size, Transform centerTarget)
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
                Vector2 neighborhoodPos = neighborhoodsCenter_static + new Vector2(((c - cols / 2f) + .5f) * neighborhood_size.x, ((r - rows / 2f) + .5f) * neighborhood_size.y);
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
        return (coords.row < neighborhoodRows_static && coords.col < neighborhoodCols_static && coords.row >= 0 && coords.col >= 0);
    }


    public static Coordinates WorldPosToNeighborhoodCoordinates(Vector2 position)
    {
        if (!neighborhoodsInitialized) InitializeNeighborhoods();
        Coordinates coords = new Coordinates();
        position = WrapPosition(position);
        coords.col = Mathf.FloorToInt((position.x - neighborhoodsCenter_static.x + neighborhoodCols_static / 2f * neighborhoodSize_static.x) / neighborhoodSize_static.x);
        coords.row = Mathf.FloorToInt((position.y - neighborhoodsCenter_static.y + neighborhoodRows_static / 2f * neighborhoodSize_static.y) / neighborhoodSize_static.y);
        coords.col = Mathf.Clamp(coords.col, 0, neighborhoodCols_static-1);
        coords.row = Mathf.Clamp(coords.row, 0, neighborhoodRows_static-1);
        return coords;
    }


    public static SurroundingsInfo GetSurroundings(Coordinates homeNeighborhoodCoords, float perceptionDistance)//int neighborhoodRadius)
    {
        int neighborhoodRadius = 1+ (Mathf.FloorToInt(perceptionDistance / neighborhoodSize_static.x));
        if (!neighborhoodsInitialized) InitializeNeighborhoods();
        if (cachedSurroundings == null) cachedSurroundings = new Dictionary<SurroundingsDefinition, SurroundingsInfo>();
        
        SurroundingsDefinition def = new SurroundingsDefinition(homeNeighborhoodCoords.row, homeNeighborhoodCoords.col, neighborhoodRadius);
        if (cachedSurroundings.ContainsKey(def)) return cachedSurroundings[def];


        LinkedList<AgentWrapped> allAgents = new LinkedList<AgentWrapped>();
       Dictionary<string, LinkedList<AgentWrapped>> sortedAgents = new Dictionary<string, LinkedList<AgentWrapped>>();

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
                }
                else if (r >= neighborhoodRows_static)
                {
                    r_wrap = r - neighborhoodRows_static;
                }
                if (c < 0)
                {
                    c_wrap = neighborhoodCols_static + c;
                }
                else if (c >= neighborhoodCols_static)
                {
                    c_wrap = c - neighborhoodCols_static;
                }

                //toDraw.Add(neighborhoods[r_wrap, c_wrap]);//.neighborhoodCenter, neighborhoodSize_static);


                Dictionary<string, List<Agent>> sourceAgents = neighborhoods[r_wrap, c_wrap].GetAgents();
                foreach (string tag in sourceAgents.Keys)
                {
                    List<Agent> agentsOut;
                    if (sourceAgents.TryGetValue(tag, out agentsOut))
                    {

                        foreach (Agent agent in agentsOut)
                        {
                            AgentWrapped wrappedAgent = new AgentWrapped(agent, WrapPosition(agent.position));
                            allAgents.AddLast(wrappedAgent);
                            if (!sortedAgents.ContainsKey(tag)) sortedAgents.Add(tag, new LinkedList<AgentWrapped>());
                            sortedAgents[tag].AddLast(wrappedAgent);
                        }
                    }
                }

                
            }
        }
        SurroundingsInfo data = new SurroundingsInfo(allAgents, sortedAgents);

        cachedSurroundings[def] = data;
        return data;
    }

    public static void AddZoneToNeighborhoods(Agent agent)
    {
        Coordinates centerCoords = WorldPosToNeighborhoodCoordinates(agent.position);
        int radius = 1 + Mathf.FloorToInt((agent.radius + Agent.forceFieldDistance) / neighborhoodSize_static.x);
        //        Debug.Log((obs.radius + Obstacle.forceFieldDistance) + " " + radius);
        for (int r = centerCoords.row - radius; r <= centerCoords.row + radius; r++)
        {
            for (int c = centerCoords.col - radius; c <= centerCoords.col + radius; c++)
            {
                if (r >= 0 && r < neighborhoodRows_static && c >= 0 && c < neighborhoodCols_static)
                {
                    Neighborhood checkNeighborhood = neighborhoods[r, c];
                    // Find the closest point to the circle within the rectangle
                    float closestX = Mathf.Clamp(agent.position.x,
                        checkNeighborhood.neighborhoodCenter.x - neighborhoodSize_static.x / 2f,
                        checkNeighborhood.neighborhoodCenter.x + neighborhoodSize_static.x / 2f);
                    float closestY = Mathf.Clamp(agent.position.y,
                        checkNeighborhood.neighborhoodCenter.y - neighborhoodSize_static.y / 2f,
                        checkNeighborhood.neighborhoodCenter.y + neighborhoodSize_static.y / 2f);

                    // Calculate the distance between the circle's center and this closest point
                    float distanceX = agent.position.x - closestX;
                    float distanceY = agent.position.y - closestY;

                    // If the distance is less than the circle's radius, an intersection occurs
                    float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
                    if (distanceSquared < ((agent.radius + Agent.forceFieldDistance) * (agent.radius + Agent.forceFieldDistance)))
                        neighborhoods[r, c].AddAgent(agent);
                }
            }
        }
    }

    public static Vector3 WrapPosition(Vector3 position)
    {
        bool mustWrap = false;
        Vector3 wrappedPosition = position;
        if (position.x < NeighborhoodCoordinator.minCorner.x) { wrappedPosition.x = NeighborhoodCoordinator.maxCorner.x + (position.x - NeighborhoodCoordinator.minCorner.x) % (neighborhoodSize_static.x * neighborhoodCols_static); mustWrap = true; }
        if (position.y < NeighborhoodCoordinator.minCorner.y) { wrappedPosition.y = NeighborhoodCoordinator.maxCorner.y + (position.y - NeighborhoodCoordinator.minCorner.y) % (neighborhoodSize_static.y * neighborhoodRows_static); mustWrap = true; }
        if (position.x > NeighborhoodCoordinator.maxCorner.x) { wrappedPosition.x = NeighborhoodCoordinator.minCorner.x + (position.x - NeighborhoodCoordinator.maxCorner.x) % (neighborhoodSize_static.x * neighborhoodCols_static); mustWrap = true; }
        if (position.y > NeighborhoodCoordinator.maxCorner.y) { wrappedPosition.y = NeighborhoodCoordinator.minCorner.y + (position.y - NeighborhoodCoordinator.maxCorner.y) % (neighborhoodSize_static.y * neighborhoodRows_static); mustWrap = true; }
        if (mustWrap) position = wrappedPosition;
        return wrappedPosition;
    }

    //if two friends are on opposite sides of the screen because one just wrapped around, they should be drawn to the edges of the screen over the wrap, not to the middle of the screen
    public static Vector3 ClosestPositionWithWrap(Vector3 myPosition, Vector3 otherPosition)
    {

        if (Mathf.Abs(myPosition.x - otherPosition.x) > NeighborhoodCoordinator.size.x / 2f)
        {
            //Debug.Log("here " + Mathf.Abs(myPosition.x - otherPosition.x) + " " + NeighborhoodCoordinator.size.x / 2f);
            otherPosition.x += NeighborhoodCoordinator.size.x * (myPosition.x > otherPosition.x ? 1 : -1);
        }
        if (Mathf.Abs(myPosition.y - otherPosition.y) > NeighborhoodCoordinator.size.y / 2f)
        {
            otherPosition.y += NeighborhoodCoordinator.size.y * (myPosition.y > otherPosition.y ? 1 : -1);
        }
        return otherPosition;
    }

    public static void WrapStationaryAgentsInEdgeNeighborhoods()
    {
        List<Agent> stationaryAgents = new List<Agent>();
        //iterate over top row
        for (int c = 0; c < neighborhoodCols_static; c++)
        {
            for (int r = 0; r < neighborhoodRows_static; r++)
            {
                stationaryAgents.AddRange(neighborhoods[r, c].GetStationaryAgents());
            }
        }

        foreach(Agent agent in stationaryAgents)
        {
            agent.ForceWrapPosition();
        }
    }
}
