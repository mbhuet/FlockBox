using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Should not be a static class because I want to be able to define boundaries without editing the script
//Class will manage itself if there are no instances by creating an instance
//



public class NeighborhoodCoordinator : MonoBehaviour {
    private static NeighborhoodCoordinator m_instance;
    public static NeighborhoodCoordinator Instance
    {
        get {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<NeighborhoodCoordinator>();
                if(m_instance == null)
                {
                    GameObject neighborhoodObj = new GameObject("NeighborhoodCoordinator");
                    m_instance = neighborhoodObj.AddComponent<NeighborhoodCoordinator>();
                }
            }
            return m_instance;
        }
        private set
        {
            m_instance = value;
        }
    }

    private Dictionary<int, List<Agent>> bucketToAgents = new Dictionary<int, List<Agent>>(); //get all agents in a bucket
    private Dictionary<Agent, List<int>> agentToBuckets = new Dictionary<Agent, List<int>>(); //get all buckets an agent is in

    public bool displayGizmos;
    [SerializeField]
    private Vector3Int dimensions = Vector3Int.one * 10;
    [SerializeField]
    private float cellSize = 10;

    public bool wrapEdges = true;

    public static bool HasMoved
    {
        get; private set;
    }

    void Awake()
    {
        if (Instance != null && Instance != this) GameObject.Destroy(this);
        else Instance = this;
    }


    public void GetSurroundings(Vector3 position, float perceptionDistance, out List<int> buckets, out List<AgentWrapped> neighbors)
    {
        neighbors = new List<AgentWrapped>();

        GetBucketsOverlappingSphere(position, perceptionDistance, out buckets);

        for (int i = 0; i < buckets.Count; i++)
        {
            if (bucketToAgents.ContainsKey(buckets[i]))
            {
                foreach(Agent agent in bucketToAgents[buckets[i]])
                {
                    neighbors.Add(new AgentWrapped(agent, wrapEdges? WrapPositionRelative(agent.Position, position) : agent.Position));
                }
            }
        }
    }


    public void BorderRepelForce(out Vector3 steer, SteeringAgent agent)
    {
        if (wrapEdges)
        {
            steer = Vector3.zero;
            return;
        }
        Vector3 futurePosition = agent.Position + agent.Velocity;
        futurePosition.x = dimensions.x > 0? Mathf.Clamp(futurePosition.x, cellSize, dimensions.x * cellSize - cellSize) : 0;
        futurePosition.y = dimensions.y > 0? Mathf.Clamp(futurePosition.y, cellSize, dimensions.y * cellSize - cellSize) : 0;
        futurePosition.z = dimensions.z > 0? Mathf.Clamp(futurePosition.z, cellSize, dimensions.z * cellSize - cellSize) : 0;

        float distanceToBorder = Mathf.Min(
            dimensions.x > 0 ? agent.Position.x : float.MaxValue,
            dimensions.y > 0 ? agent.Position.y : float.MaxValue,
            dimensions.z > 0 ? agent.Position.z : float.MaxValue,
            dimensions.x > 0 ? dimensions.x * cellSize - agent.Position.x : float.MaxValue,
            dimensions.y > 0 ? dimensions.y * cellSize - agent.Position.y : float.MaxValue,
            dimensions.z > 0 ? dimensions.z * cellSize - agent.Position.z : float.MaxValue);
        if (distanceToBorder <= 0) distanceToBorder = .001f;

        agent.GetSeekVector(out steer, futurePosition);
        steer *= cellSize / distanceToBorder;
    }


    public void UpdateAgentBuckets(Agent agent, out List<int> buckets)
    {
        RemoveAgentFromBuckets(agent, out buckets);
        AddAgentToBuckets(agent, out buckets);
    }

    public void RemoveAgentFromBuckets(Agent agent, out List<int> buckets)
    {
        if (agentToBuckets.TryGetValue(agent, out buckets))
        {
            for (int i = 0; i < buckets.Count; i++)
            {
                if (bucketToAgents.ContainsKey(buckets[i]))
                {
                    bucketToAgents[buckets[i]].Remove(agent);
                }
            }
            agentToBuckets[agent].Clear();
        }

    }

    private void AddAgentToBuckets(Agent agent, out List<int> buckets)
    {
        if (!agentToBuckets.ContainsKey(agent))
        {
            agentToBuckets.Add(agent, new List<int>());
        }

        switch (agent.neighborType)
        {
            case Agent.NeighborType.SHERE:
                GetBucketsOverlappingSphere(agent.Position, agent.Radius, out buckets);
                break;
            case Agent.NeighborType.POINT:
                buckets = new List<int>() { GetBucketOverlappingPoint(agent.Position) };
                break;
            default:
                buckets = new List<int>() { GetBucketOverlappingPoint(agent.Position) };
                break;
        }
        for (int i = 0; i < buckets.Count; i++)
            {
                if (!bucketToAgents.ContainsKey(buckets[i]))
                {
                    bucketToAgents.Add(buckets[i], new List<Agent>());
                }
                bucketToAgents[buckets[i]].Add(agent);
                agentToBuckets[agent].Add(buckets[i]);
            }
        
        
    }

    public int GetBucketOverlappingPoint(Vector3 point)
    {
        return GetHash(wrapEdges? WrapPosition(point) : point);
    }

    public void GetBucketsOverlappingSphere(Vector3 center, float radius, out List<int> buckets)
    {
        int neighborhoodRadius = 1 + Mathf.FloorToInt((radius-.01f) / cellSize);
        buckets = new List<int>();
        Vector3 positionContainer;

        for (int xOff = -neighborhoodRadius; xOff < neighborhoodRadius; xOff++)
        {
            for (int yOff = -neighborhoodRadius; yOff < neighborhoodRadius; yOff++)
            {
                for (int zOff = -neighborhoodRadius; zOff < neighborhoodRadius; zOff++)
                {

                    positionContainer.x = (center.x + xOff * cellSize);
                    positionContainer.y = (center.y + yOff * cellSize);
                    positionContainer.z = (center.z + zOff * cellSize);
                    if (!wrapEdges)
                    {
                        if (   positionContainer.x < 0 || positionContainer.x > dimensions.x * cellSize
                            || positionContainer.y < 0 || positionContainer.y > dimensions.y * cellSize
                            || positionContainer.z < 0 || positionContainer.z > dimensions.z * cellSize)
                        {
                            continue;
                        }
                        else
                        {
                            buckets.Add(GetHash(positionContainer));
                        }
                    }
                    else
                    {
                        buckets.Add(GetHash(WrapPosition(positionContainer)));
                    }
                }
            }
        }

    }


    public Vector3 WrapPositionRelative(Vector3 position, Vector3 relativeTo)
    {
        // |-* |   |   |   | *-|
        if (relativeTo.x > position.x && (relativeTo.x - position.x > (position.x + dimensions.x * cellSize) - relativeTo.x)){
            position.x = position.x + dimensions.x * cellSize;
        }
        else if(relativeTo.x < position.x && (position.x - relativeTo.x > (relativeTo.x + dimensions.x * cellSize) - position.x))
        {
            position.x = position.x - dimensions.x * cellSize;
        }

        if (relativeTo.y > position.y && (relativeTo.y - position.y > (position.y + dimensions.y * cellSize) - relativeTo.y))
        {
            position.y = position.y + dimensions.y * cellSize;
        }
        else if (relativeTo.y < position.y && (position.y - relativeTo.y > (relativeTo.y + dimensions.y * cellSize) - position.y))
        {
            position.y = position.y - dimensions.y * cellSize;
        }

        if (relativeTo.z > position.z && (relativeTo.z - position.z > (position.z + dimensions.z * cellSize) - relativeTo.z))
        {
            position.z = position.z + dimensions.z * cellSize;
        }
        else if (relativeTo.z < position.z && (position.z - relativeTo.z > (relativeTo.z + dimensions.z * cellSize) - position.z))
        {
            position.z = position.z - dimensions.z * cellSize;
        }
        return position;
    }


    public void ValidatePosition(ref Vector3 position)
    {
        if (wrapEdges) position = WrapPosition(position);
        else
        {
            if (position.x < 0) position.x = 0;
            else if (position.x > dimensions.x * cellSize) position.x = dimensions.x * cellSize;
            if (position.y < 0) position.y = 0;
            else if (position.y > dimensions.y * cellSize) position.y = dimensions.y * cellSize;
            if (position.z < 0) position.z = 0;
            else if (position.z > dimensions.z * cellSize) position.z = dimensions.z * cellSize;
        }
    }

    private Vector3 WrapPosition(Vector3 position)
    {
        if (dimensions.x == 0)
        {
            position.x = 0;
        }
        else if (position.x < 0)
        {
            position.x = dimensions.x * cellSize + position.x;
        }
        else if (position.x > dimensions.x * cellSize)
        {
            position.x = position.x % (dimensions.x * cellSize);
        }

        if (dimensions.y == 0)
        {
            position.y = 0;
        }
        else if (position.y < 0)
        {
            position.y = dimensions.y * cellSize + position.y;
        }
        else if (position.y > dimensions.y * cellSize)
        {
            position.y = position.y % (dimensions.y * cellSize);
        }

        if (dimensions.z == 0)
        {
            position.z = 0;
        }
        else if (position.z < 0)
        {
            position.z = dimensions.z * cellSize + position.z;
        }
        else if (position.z > dimensions.z * cellSize)
        {
            position.z = position.z % (dimensions.z * cellSize);
        }


        return position;
    }


    public Vector3 RandomPosition()
    {
        return new Vector3(
           UnityEngine.Random.Range(0, dimensions.x * cellSize),
           UnityEngine.Random.Range(0, dimensions.y * cellSize),
           UnityEngine.Random.Range(0, dimensions.z * cellSize)
         );
    }



    private int GetHash(float x, float y, float z)
    {
        if (x < 0 || y < 0 || z < 0) return -1;
        return (int)(
             Mathf.Floor(x / cellSize)
           + Mathf.Floor(y / cellSize) * dimensions.x
           + Mathf.Floor(z / cellSize) * dimensions.x * dimensions.y);
    }

    private int GetHash(Vector3 position)
    {
        return GetHash(position.x, position.y, position.z);
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (displayGizmos) DrawNeighborHoods();
    }

    void DrawNeighborHoods()
    {
        if (bucketToAgents == null) return;
        Gizmos.color = Color.grey;

        Gizmos.DrawWireCube((Vector3)dimensions * (cellSize / 2f), (Vector3)dimensions * cellSize);
        Gizmos.color = Color.grey * .1f;


        for (int x = 0; x < dimensions.x; x++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int z = 0; z < dimensions.z; z++)
                {
                    Vector3 corner = new Vector3(x, y, z) * cellSize;
                    int bucket = GetHash(corner);
                    if (bucketToAgents.ContainsKey(bucket) && bucketToAgents[bucket].Count > 0)
                    {
                        Gizmos.DrawCube(corner + Vector3.one * (cellSize / 2f), Vector3.one * cellSize);
                    }
                }
            }
        }
    }
#endif

}
