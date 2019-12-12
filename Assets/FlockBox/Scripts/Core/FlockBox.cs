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

        private Dictionary<Agent, string> lastKnownTag = new Dictionary<Agent, string>();
        private Dictionary<string, List<Agent>> tagToAgents = new Dictionary<string, List<Agent>>();



        [SerializeField]
        private int dimensions_x = 10;
        [SerializeField]
        private int dimensions_y = 10;
        [SerializeField]
        private int dimensions_z = 10;
        [SerializeField]
        private float cellSize = 10;


        private Vector3 _worldDimensions = Vector3.zero;
        public Vector3 WorldDimensions
        {
            get { return _worldDimensions; } private set { _worldDimensions = value; }
        }

        public bool wrapEdges = false;
        public float boundaryBuffer = 10;
        public float sleepChance;
        [SerializeField] private int maxCellCapacity = 10;
        [SerializeField] private bool capCellCapacity = true;

        [Serializable]
        public struct AgentPopulation
        {
            public Agent prefab;
            public int population;
        }
        public List<AgentPopulation> startingPopulations;

        [SerializeField]
        private bool drawGizmos = true;


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


        public void GetSurroundings(Vector3 position, Vector3 velocity, List<int> buckets, SurroundingsContainer surroundings)
        {
            
            if(buckets == null) buckets = new List<int>();
            else buckets.Clear();

            if (surroundings.perceptionRadius > 0)
            {
                GetBucketsOverlappingSphere(position, surroundings.perceptionRadius, buckets);
            }
            if (surroundings.lookAheadSeconds > 0)
            {
                GetBucketsOverlappingLine(position, position + velocity * surroundings.lookAheadSeconds, buckets);
            }

            surroundings.allAgents = new List<Agent>();

            for (int i = 0; i < buckets.Count; i++)
            {
                if (bucketToAgents.ContainsKey(buckets[i]))
                {
                    surroundings.allAgents.AddRange(bucketToAgents[buckets[i]]);
                }
            }
            if (surroundings.globalSearchTags.Count > 0)
            {
                foreach (string agentTag in surroundings.globalSearchTags) {
                    if (tagToAgents.ContainsKey(agentTag))
                    {
                        surroundings.allAgents.AddRange(tagToAgents[agentTag]);
                    }
                }
            }
        }

        /// <summary>
        /// Remove from 
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="buckets"></param>
        /// <param name="isStatic">Will this agent be updating its position every frame</param>
        public void UpdateAgentBuckets(Agent agent, List<int> buckets, bool isStatic)
        {
            if(agent.shape.type == Shape.ShapeType.POINT)
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
                                bucketToAgents.Add(currentbucket, new List<Agent>(maxCellCapacity) { agent });
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
            RemoveAgentFromBuckets(agent, buckets);
            AddAgentToBuckets(agent, buckets, isStatic);
        }

        public void RemoveAgentFromBuckets(Agent agent, List<int> buckets)
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

        private void UpdateTagRegistration(Agent agent)
        {

        }

        private List<Agent> _bucketContents;
        private void AddAgentToBuckets(Agent agent, List<int> buckets, bool isStatic)
        {
            if (!agentToBuckets.ContainsKey(agent))
            {
                agentToBuckets.Add(agent, new List<int>());
            }
            if (!lastKnownTag.ContainsKey(agent))
            {
                lastKnownTag.Add(agent, agent.tag);
                if (!tagToAgents.ContainsKey(agent.tag))
                {
                    tagToAgents.Add(agent.tag, new List<Agent>());
                }
                tagToAgents[agent.tag].Add(agent);

            }
            else if (!agent.CompareTag(lastKnownTag[agent]))
            {
                tagToAgents[lastKnownTag[agent]].Remove(agent);
                lastKnownTag[agent] = agent.tag;
                if (!tagToAgents.ContainsKey(agent.tag))
                {
                    tagToAgents.Add(agent.tag, new List<Agent>());
                }
                tagToAgents[agent.tag].Add(agent);
            }

            buckets = new List<int>();

            switch (agent.shape.type)
            {
                case Shape.ShapeType.SPHERE:
                    GetBucketsOverlappingSphere(agent.Position, agent.shape.radius, buckets);
                    break;
                case Shape.ShapeType.POINT:
                    buckets = new List<int>() { GetBucketOverlappingPoint(agent.Position) };
                    break;
                case Shape.ShapeType.LINE:
                    GetBucketsOverlappingLine(agent.Position, agent.Position + agent.transform.localRotation * Vector3.forward * agent.shape.length, buckets);
                    break;
                case Shape.ShapeType.CYLINDER:
                    GetBucketsOverlappingCylinder(agent.Position, agent.Position + agent.transform.localRotation * Vector3.forward * agent.shape.length, agent.shape.radius, buckets);
                    break;
                default:
                    buckets = new List<int>() { GetBucketOverlappingPoint(agent.Position) };
                    break;
            }
            for (int i = 0; i < buckets.Count; i++)
            {
                if (!bucketToAgents.ContainsKey(buckets[i]))
                {
                    bucketToAgents.Add(buckets[i], new List<Agent>(maxCellCapacity));
                }
                _bucketContents = bucketToAgents[buckets[i]];
                if(!capCellCapacity || isStatic || (_bucketContents.Count<maxCellCapacity))
                {
                    _bucketContents.Add(agent);
                    agentToBuckets[agent].Add(buckets[i]);
                }

            }


        }

        public int GetBucketOverlappingPoint(Vector3 point)
        {
            return WorldPositionToHash(point);
        }

        public void GetBucketsOverlappingLine(Vector3 start, Vector3 end, List<int> buckets)
        {
            
            int x0 = ToCellFloor(start.x);
            int x1 = ToCellFloor(end.x);

            int y0 = ToCellFloor(start.y);
            int y1 = ToCellFloor(end.y);

            int z0 = ToCellFloor(start.z);
            int z1 = ToCellFloor(end.z);     


            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int dz = Math.Abs(z1 - z0), sz = z0 < z1 ? 1 : -1;
            int dm = Mathf.Max(dx, dy, dz), i = dm; /* maximum difference */
            x1 = y1 = z1 = dm / 2; /* error offset */

            buckets.Capacity = buckets.Count + dm;
            buckets.Add(CellPositionToHash(x0, y0, z0));


            for (; ; )
            {  /* loop */
                
                if (dimensions_x > 0 && (x0 < 0 || x0 >= dimensions_x)) break;
                if (dimensions_y > 0 && (y0 < 0 || y0 >= dimensions_y)) break;
                if (dimensions_z > 0 && (z0 < 0 || z0 >= dimensions_z)) break;
                buckets.Add(CellPositionToHash(x0, y0, z0));

                if (i-- == 0) break;


                x1 -= dx; if (x1 < 0) { x1 += dm; x0 += sx; }
                y1 -= dy; if (y1 < 0) { y1 += dm; y0 += sy; }
                z1 -= dz; if (z1 < 0) { z1 += dm; z0 += sz; }
            }

        }

        
        public void GetBucketsOverlappingCylinder(Vector3 a, Vector3 b, float r, List<int> buckets)
        {
            Vector3 min = Vector3.Min(a, b) - Vector3.one * r;
            Vector3 max = Vector3.Max(a, b) + Vector3.one * r;

            Vector3Int minCell = ToCellFloor(min);
            Vector3Int maxCell = ToCellFloor(max);

            for(int x = minCell.x; x<=maxCell.x; x++)
            {
                for(int y = minCell.y; y<=maxCell.y; y++)
                {
                    for(int z = minCell.z; z<=maxCell.z; z++)
                    {
                        if (x < 0 || x > dimensions_x
                        || y < 0 || y > dimensions_y
                        || z < 0 || z > dimensions_z)
                        {
                            continue;
                        }
                        buckets.Add(CellPositionToHash(x, y, z));
                    }
                }
            }


        }

        public void GetBucketsOverlappingSphere(Vector3 center, float radius, List<int> buckets)
        {
            int neighborhoodRadius = 1 + (int)((radius - .01f) / cellSize);
            if(buckets == null) buckets = new List<int>();
            buckets.Capacity = buckets.Count + ((neighborhoodRadius*2+1) * (neighborhoodRadius*2+1) * (neighborhoodRadius*2+1));
            int center_x = ToCellFloor(center.x);
            int center_y = ToCellFloor(center.y);
            int center_z = ToCellFloor(center.z);

            for (int x = center_x - neighborhoodRadius; x <= center_x + neighborhoodRadius; x++)
            {
                for (int y = center_y - neighborhoodRadius; y <= center_y + neighborhoodRadius; y++)
                {
                    for (int z = center_z - neighborhoodRadius; z <= center_z + neighborhoodRadius; z++)
                    {
                        if (x < 0 || x > dimensions_x
                                || y < 0 || y > dimensions_y
                                || z < 0 || z > dimensions_z)
                        {
                            continue;
                        }
                        buckets.Add(CellPositionToHash(x, y, z));
                        
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

        private Vector3Int ToCellFloor(Vector3 position)
        {
            return new Vector3Int(ToCellFloor(position.x), ToCellFloor(position.y), ToCellFloor(position.z));
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
            if (drawGizmos)
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
