using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Should not be a static class because I want to be able to define boundaries without editing the script
//Class will manage itself if there are no instances by creating an instance
//



public class NeighborhoodCoordinator : MonoBehaviour {
    public static NeighborhoodCoordinator Instance;

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
        //If there is an instance of NeighborhoodCoordinator in the scene, it will assign itself 
        if (Instance != null && Instance != this) GameObject.Destroy(this);
        else
        {
            Instance = this;
        }
    }





    public void GetSurroundingsWrapped(Vector3 position, float perceptionDistance, out List<int> buckets, out List<AgentWrapped> neighbors)
    {
        neighbors = new List<AgentWrapped>();

        GetBucketsOverlappingSphere(position, perceptionDistance, out buckets);

        for (int i = 0; i < buckets.Count; i++)
        {
            if (bucketToAgents.ContainsKey(i))
            {
                foreach(Agent agent in bucketToAgents[i])
                {
                    neighbors.Add(new AgentWrapped(agent, WrapPositionRelative(agent.Position, position)));
                }
            }
        }
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
        return GetHash(WrapPosition(point));
    }

    public void GetBucketsOverlappingSphere(Vector3 center, float radius, out List<int> buckets)
    {
        //if radius == cellSize, save some time
        int neighborhoodRadius = 1 + Mathf.FloorToInt((radius) / cellSize);
        buckets = new List<int>();
        Vector3 positionContainer;

        for (int xOff = -neighborhoodRadius; xOff <= neighborhoodRadius; xOff++)
        {
            for (int yOff = -neighborhoodRadius; yOff <= neighborhoodRadius; yOff++)
            {
                for (int zOff = -neighborhoodRadius; zOff <= neighborhoodRadius; zOff++)
                {
                    positionContainer.x = (center.x + xOff * cellSize);
                    positionContainer.y = (center.y + yOff * cellSize);
                    positionContainer.z = (center.z + zOff * cellSize);
                    buckets.Add(GetHash(WrapPosition(positionContainer)));
                }
            }
        }

    }


    public Vector3 WrapPositionRelative(Vector3 position, Vector3 relativeTo)
    {
        return position;
    }

    public Vector3 WrapPosition(Vector3 position)
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
            position.x = position.x % dimensions.x * cellSize;
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
            position.y = position.y % dimensions.y * cellSize;
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
            position.z = position.z % dimensions.z * cellSize;
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
        return 
             Mathf.FloorToInt(x / cellSize)
           + Mathf.FloorToInt(y / cellSize) * dimensions.x
           + Mathf.FloorToInt(z / cellSize) * dimensions.x * dimensions.y;
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
                    else
                    {
                        //Gizmos.DrawWireCube(corner + Vector3.one * (cellSize / 2f), Vector3.one * cellSize);

                    }
                }
            }
        }
    }
#endif

}
