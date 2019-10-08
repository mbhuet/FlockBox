using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Should not be a static class because I want to be able to define boundaries without editing the script
//Class will manage itself if there are no instances by creating an instance
//


namespace CloudFine
{
    public class FlockBox : MonoBehaviour
    {
        private Dictionary<int, List<Agent>> bucketToAgents = new Dictionary<int, List<Agent>>(); //get all agents in a bucket
        private Dictionary<Agent, List<int>> agentToBuckets = new Dictionary<Agent, List<int>>(); //get all buckets an agent is in
        private int defaultBucketCapacity = 10;

        public bool displayGizmos;
        [SerializeField]
        private int dimensions_x = 10;
        [SerializeField]
        private int dimensions_y = 10;
        [SerializeField]
        private int dimensions_z = 10;

        private Vector3 _worldDimensions = Vector3.zero;
        public Vector3 WorldDimensions
        {
            get { return _worldDimensions; } private set { _worldDimensions = value; }
        }
        [SerializeField]
        private float cellSize = 10;


        public bool wrapEdges = true;
        public float boundaryBuffer = 10;


        [Serializable]
        public struct AgentPopulation
        {
            public Agent prefab;
            public int population;
        }
        public List<AgentPopulation> startingPopulations;


        void Start()
        {
            foreach (AgentPopulation pop in startingPopulations)
            {
                if (pop.prefab == null) return;
                for (int i = 0; i < pop.population; i++)
                {
                    Agent agent = GameObject.Instantiate<Agent>(pop.prefab);
                    agent.Spawn(this);
                }
            }
        }

        private void Update()
        {
            _worldDimensions.x = dimensions_x * cellSize;
            _worldDimensions.y = dimensions_y * cellSize;
            _worldDimensions.z = dimensions_z * cellSize;

#if UNITY_EDITOR
            //clear here instead of in OnDrawGizmos so that they persist when the editor is paused
            bucketsToDraw.Clear();
#endif
        }


        public void GetSurroundings(Vector3 position, Vector3 velocity, ref List<int> buckets, SurroundingsContainer surroundings)
        {
            
            if(buckets == null) buckets = new List<int>();
            else buckets.Clear();

            if (surroundings.perceptionRadius > 0)
            {
                GetBucketsOverlappingSphere(position, surroundings.perceptionRadius, ref buckets);
            }
            if (surroundings.lookAheadSeconds > 0)
            {
                GetBucketsOverlappingLine(position, position + velocity * surroundings.lookAheadSeconds, 0, ref buckets);
            }

            surroundings.allAgents = new List<Agent>(buckets.Count * defaultBucketCapacity);

            for (int i = 0; i < buckets.Count; i++)
            {
                if (bucketToAgents.ContainsKey(buckets[i]))
                {
                    surroundings.allAgents.AddRange(bucketToAgents[buckets[i]]);
                }
            }
        }

        public void UpdateAgentBuckets(Agent agent, out List<int> buckets)
        {
            if(agent.neighborType == Agent.NeighborType.POINT)
            {
                if(agentToBuckets.TryGetValue(agent, out buckets))
                {
                    if (buckets.Count == 1)
                    {
                        int currentbucket = GetBucketOverlappingPoint(agent.Position);
                        int lastBucket = agentToBuckets[agent][0];
                        if (lastBucket != currentbucket)
                        {
                            bucketToAgents[lastBucket].Remove(agent);
                            if (!bucketToAgents.ContainsKey(currentbucket))
                            {
                                bucketToAgents.Add(currentbucket, new List<Agent>(defaultBucketCapacity) { agent });
                            }
                            else
                            {
                                bucketToAgents[currentbucket].Add(agent);
                            }
                            agentToBuckets[agent][0] = currentbucket;
                        }
                        return;
                    }
                }
            }
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
            buckets = new List<int>();

            switch (agent.neighborType)
            {
                case Agent.NeighborType.SHERE:
                    GetBucketsOverlappingSphere(agent.Position, agent.Radius, ref buckets);
                    break;
                case Agent.NeighborType.POINT:
                    buckets = new List<int>() { GetBucketOverlappingPoint(agent.Position) };
                    break;
                case Agent.NeighborType.LINE:
                    GetBucketsOverlappingLine(agent.Position, agent.Position + agent.Forward, agent.Radius, ref buckets);
                    break;
                default:
                    buckets = new List<int>() { GetBucketOverlappingPoint(agent.Position) };
                    break;
            }
            for (int i = 0; i < buckets.Count; i++)
            {
                if (!bucketToAgents.ContainsKey(buckets[i]))
                {
                    bucketToAgents.Add(buckets[i], new List<Agent>(defaultBucketCapacity));
                }
                bucketToAgents[buckets[i]].Add(agent);
                agentToBuckets[agent].Add(buckets[i]);
            }


        }

        public int GetBucketOverlappingPoint(Vector3 point)
        {
            return WorldPositionToHash(point);
        }

        public void GetBucketsOverlappingLine(Vector3 start, Vector3 end, float thickness, ref List<int> buckets)
        {
            int x0 = ToCellFloor(start.x);
            int x1 = ToCellFloor(end.x);

            int y0 = ToCellFloor(start.y);
            int y1 = ToCellFloor(end.y);

            int z0 = ToCellFloor(start.z);
            int z1 = ToCellFloor(end.z);

            float wd = thickness;
            

            if (buckets == null) buckets = new List<int>();

            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int dz = Math.Abs(z1 - z0), sz = z0 < z1 ? 1 : -1;
            int dm = Mathf.Max(dx, dy, dz), i = dm; /* maximum difference */
            x1 = y1 = z1 = dm / 2; /* error offset */

            buckets.Capacity = buckets.Count + dm;
            buckets.Add(CellPositionToHash(x0, y0, z0));


            for (; ; )
            {  /* loop */
                
                if (x0 < 0 || x0 >= dimensions_x) break;
                if (y0 < 0 || y0 >= dimensions_y) break;
                if (z0 < 0 || z0 >= dimensions_z) break;
                buckets.Add(CellPositionToHash(x0, y0, z0));

                if (i-- == 0) break;


                x1 -= dx; if (x1 < 0) { x1 += dm; x0 += sx; }
                y1 -= dy; if (y1 < 0) { y1 += dm; y0 += sy; }
                z1 -= dz; if (z1 < 0) { z1 += dm; z0 += sz; }
            }

        }

        public void GetBucketsOverlappingSphere(Vector3 center, float radius, ref List<int> buckets)
        {
            int neighborhoodRadius = 1 + (int)((radius - .01f) / cellSize);
            if(buckets == null) buckets = new List<int>();
            buckets.Capacity = buckets.Count + ((neighborhoodRadius*2+1) * (neighborhoodRadius*2+1) * (neighborhoodRadius*2+1));
            int center_x = ToCellFloor(center.x);
            int center_y = ToCellFloor(center.y);
            int center_z = ToCellFloor(center.z);

            for (int xOff = center_x - neighborhoodRadius; xOff <= center_x + neighborhoodRadius; xOff++)
            {
                for (int yOff = center_y - neighborhoodRadius; yOff <= center_y + neighborhoodRadius; yOff++)
                {
                    for (int zOff = center_z - neighborhoodRadius; zOff <= center_z + neighborhoodRadius; zOff++)
                    {
                        if (xOff < 0 || xOff > dimensions_x
                                || yOff < 0 || yOff > dimensions_y
                                || zOff < 0 || zOff > dimensions_z)
                        {
                            continue;
                        }
                        buckets.Add(CellPositionToHash(xOff, yOff, zOff));
                        
                    }
                }
            }

        }

        public Vector3 WrapPositionRelative(Vector3 position, Vector3 relativeTo)
        {
            // |-* |   |   |   | *-|
            if (relativeTo.x > position.x && (relativeTo.x - position.x > (position.x + dimensions_x * cellSize) - relativeTo.x))
            {
                position.x = position.x + dimensions_x * cellSize;
            }
            else if (relativeTo.x < position.x && (position.x - relativeTo.x > (relativeTo.x + dimensions_x * cellSize) - position.x))
            {
                position.x = position.x - dimensions_x * cellSize;
            }

            if (relativeTo.y > position.y && (relativeTo.y - position.y > (position.y + dimensions_y * cellSize) - relativeTo.y))
            {
                position.y = position.y + dimensions_y * cellSize;
            }
            else if (relativeTo.y < position.y && (position.y - relativeTo.y > (relativeTo.y + dimensions_y * cellSize) - position.y))
            {
                position.y = position.y - dimensions_y * cellSize;
            }

            if (relativeTo.z > position.z && (relativeTo.z - position.z > (position.z + dimensions_z * cellSize) - relativeTo.z))
            {
                position.z = position.z + dimensions_z * cellSize;
            }
            else if (relativeTo.z < position.z && (position.z - relativeTo.z > (relativeTo.z + dimensions_z * cellSize) - position.z))
            {
                position.z = position.z - dimensions_z * cellSize;
            }
            return position;
        }

        public void ValidatePosition(ref Vector3 position)
        {
            if (dimensions_x <= 0)
            {
                position.x = 0;
            }
            else
            {
                if (position.x < 0)
                {
                    position.x = wrapEdges ? dimensions_x * cellSize + position.x : 0;
                }
                else if (position.x > dimensions_x * cellSize)
                {
                    position.x = wrapEdges ? position.x % (dimensions_x * cellSize) : dimensions_x * cellSize;
                }
            }
            if (dimensions_y <= 0)
            {
                position.y = 0;
            }
            else
            {
                if (position.y < 0)
                {
                    position.y = wrapEdges ? dimensions_y * cellSize + position.y : 0;
                }
                else if (position.y > dimensions_y * cellSize)
                {
                    position.y = wrapEdges ? position.y % (dimensions_y * cellSize) : dimensions_y * cellSize;
                }
            }

            if (dimensions_z <= 0)
            {
                position.z = 0;
            }
            else { 
                if (position.z < 0)
                {
                    position.z = wrapEdges ? dimensions_z * cellSize + position.z : 0;
                }
                else if (position.z > dimensions_z * cellSize)
                {
                    position.z = wrapEdges ? position.z % (dimensions_z * cellSize) : dimensions_z * cellSize;
                }
            }
        }

        public void ValidateVelocity(ref Vector3 velocity)
        {
            if (dimensions_x <= 0) velocity.x = 0;
            if (dimensions_y <= 0) velocity.y = 0;
            if (dimensions_z <= 0) velocity.z = 0;
        }


        public Vector3 RandomPosition()
        {
            float buffer = wrapEdges ? 0 : boundaryBuffer;
            return new Vector3(
               UnityEngine.Random.Range(buffer, dimensions_x * cellSize - buffer),
               UnityEngine.Random.Range(buffer, dimensions_y * cellSize - buffer),
               UnityEngine.Random.Range(buffer, dimensions_z * cellSize - buffer)
             );
        }

        private int ToCellFloor(float p)
        {
            return (int)(p / cellSize);
        }

        private int CellPositionToHash(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0) return -1;

            return (
                 x
               + y * (dimensions_x + 1) // +1 in case dimension is 0, will still produce unique hash
               + z * (dimensions_x + 1) * (dimensions_y + 1));
        }

        private int WorldPositionToHash(float x, float y, float z)
        {
            return CellPositionToHash((int)(x / cellSize), (int)(y / cellSize), (int)(z / cellSize));
        }

        private int WorldPositionToHash(Vector3 position)
        {
            return WorldPositionToHash(position.x, position.y, position.z);
        }

#if UNITY_EDITOR

        private List<int> bucketsToDraw = new List<int>(); //useful for debugging

        private void OnDrawGizmos()
        {
            if (displayGizmos)
            {
                Gizmos.color = Color.grey;
                Gizmos.matrix = this.transform.localToWorldMatrix;
                Vector3 dimensions = new Vector3(dimensions_x, dimensions_y, dimensions_z);
                Gizmos.DrawWireCube((Vector3)dimensions * (cellSize / 2f), (Vector3)dimensions * cellSize);
                if (!wrapEdges)
                {
                    Gizmos.color = Color.yellow * .75f;
                    Gizmos.DrawWireCube((Vector3)dimensions * (cellSize / 2f),
                        new Vector3(
                            Mathf.Max(0, dimensions_x * cellSize - boundaryBuffer * 2f),
                            Mathf.Max(0, dimensions_y * cellSize - boundaryBuffer * 2f),
                            Mathf.Max(0, dimensions_z * cellSize - boundaryBuffer * 2f)));
                }
                DrawNeighborHoods();
            }
        }

        void DrawNeighborHoods()
        {
            if (bucketToAgents == null) return;
            
            Gizmos.color = Color.grey * .1f;


            for (int x = 0; x < (dimensions_x > 0 ? dimensions_x : 1); x++)
            {
                for (int y = 0; y < (dimensions_y > 0 ? dimensions_y : 1); y++)
                {
                    for (int z = 0; z < (dimensions_z > 0 ? dimensions_z : 1); z++)
                    {

                        Vector3 corner = new Vector3(x, y, z) * cellSize;
                        int bucket = WorldPositionToHash(corner);

                        if (bucketsToDraw.Contains(bucket))
                        {
                            Gizmos.color = Color.red * .8f;
                            Gizmos.DrawCube(corner + Vector3.one * (cellSize / 2f), Vector3.one * cellSize);
                        }
                        else if (bucketToAgents.ContainsKey(bucket) && bucketToAgents[bucket].Count > 0)
                        {
                            Gizmos.color = Color.grey * .1f;
                            Gizmos.DrawCube(corner + Vector3.one * (cellSize / 2f), Vector3.one * cellSize);
                        }
                    }
                }
            }
        }

#endif
    }
}
