using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using CloudFine.FlockBox.DOTS;
using Unity.Transforms;
using UnityEngine.Jobs;
using System.ComponentModel.Design.Serialization;

namespace CloudFine.FlockBox
{
    public class FlockBox : MonoBehaviour
    {
        public static Action<FlockBox> OnValuesModified;

        private Dictionary<int, HashSet<Agent>> bucketToAgents = new Dictionary<int, HashSet<Agent>>(); //get all agents in a bucket
        private Dictionary<Agent, HashSet<int>> agentToBuckets = new Dictionary<Agent, HashSet<int>>(); //get all buckets an agent is in

        private Dictionary<Agent, string> lastKnownTag = new Dictionary<Agent, string>();
        private Dictionary<string, HashSet<Agent>> tagToAgents = new Dictionary<string, HashSet<Agent>>();


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
        public int DimensionX
        {
            get { return dimensions_x; }
        }
        public int DimensionY
        {
            get { return dimensions_y; }
        }
        public int DimensionZ
        {
            get { return dimensions_z; }
        }
        public float CellSize
        {
            get { return cellSize; }
        }
        public int TotalCells
        {
            get { return (Mathf.Max(dimensions_x, 1) * Mathf.Max(dimensions_y, 1) * Mathf.Max(dimensions_z, 1)); }
        }

        public int CellCapacity
        {
            get
            {
                if (!capCellCapacity) return int.MaxValue;
                return maxCellCapacity;
            }
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

        [SerializeField]
        private bool useDOTS = false;

        private Entity agentEntityPrefab;
        private Entity syncedEntityTransform
        {
            get
            {
                if (_syncedEntityTransform == Entity.Null)
                {
                    _syncedEntityTransform = CreateSyncedRoot();
                }
                return _syncedEntityTransform;
            }
        }
        private Entity _syncedEntityTransform;


        #region DOTS

        private Entity CreateSyncedRoot()
        {
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            //
            // Set up root entity that will follow FlockBox GameObject
            //
            Entity root = manager.CreateEntity(new ComponentType[]{
                        typeof(LocalToWorld),
                    });

            manager.AddComponentObject(root, this.transform);
            manager.AddComponentData<CopyTransformFromGameObject>(root, new CopyTransformFromGameObject { });
            return root;
        }

        private void InstantiateAgentEntitiesFromPrefab(Agent prefab, int population)
        {
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            //
            //Create entity template for agents with Conversion System
            //
            GameObjectConversionSettings settings = new GameObjectConversionSettings()
            {
                DestinationWorld = World.DefaultGameObjectInjectionWorld
            };
            agentEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab.gameObject, settings);
            NativeArray<Entity> agents = new NativeArray<Entity>(population, Allocator.TempJob);
            manager.Instantiate(agentEntityPrefab, agents);

            for (int i = 0; i < population; i++)
            {
                Entity entity = agents[i];
                AgentData data = manager.GetComponentData<AgentData>(entity);
                data.Position = RandomPosition();
                data.Velocity = UnityEngine.Random.insideUnitSphere;
                data.UniqueID = (int)(UnityEngine.Random.value * 100000);
                manager.SetComponentData(entity, data);

                //parent the agent to the flockbox root
                manager.AddComponentData<Parent>(entity, new Parent { Value = syncedEntityTransform });
                manager.AddComponentData<LocalToParent>(entity, new LocalToParent());

                manager.AddSharedComponentData<FlockData>(entity, new FlockData { Flock = this });
                manager.AddComponentData<BoundaryData>(entity, new BoundaryData { Dimensions = WorldDimensions, Margin = boundaryBuffer, Wrap = wrapEdges });
                //add all component data, imitate agent.Spawn(this)
            }
            manager.DestroyEntity(agentEntityPrefab);
            agents.Dispose();
        }


        #endregion

        private void Awake()
        {
            _worldDimensions.x = dimensions_x * cellSize;
            _worldDimensions.y = dimensions_y * cellSize;
            _worldDimensions.z = dimensions_z * cellSize;
        }

        void Start()
        {
            foreach (AgentPopulation pop in startingPopulations)
            {
                if (pop.prefab == null) continue;

                if (useDOTS)
                {
                    InstantiateAgentEntitiesFromPrefab(pop.prefab, pop.population);
                }
                else
                {
                    for(int i =0; i<pop.population; i++)
                    {
                        Agent agent = GameObject.Instantiate(pop.prefab);
                        agent.Spawn(this);
                    }
                }
            }
        }

        private void Update()
        {
            if (transform.hasChanged)
            {
                MarkAsChanged();
                transform.hasChanged = false;
            }
            _worldDimensions.x = dimensions_x * cellSize;
            _worldDimensions.y = dimensions_y * cellSize;
            _worldDimensions.z = dimensions_z * cellSize;

#if UNITY_EDITOR
            //clear here instead of in OnDrawGizmos so that they persist when the editor is paused
            bucketsToDebugDraw.Clear();
#endif
        }

        private void OnValidate()
        {
            MarkAsChanged();
        }

        public void MarkAsChanged()
        {
            if (OnValuesModified != null) OnValuesModified.Invoke(this);
        }



        public void GetSurroundings(Vector3 position, Vector3 velocity, HashSet<int> buckets, SurroundingsContainer surroundings)
        {

            if (buckets==null)
            {
                buckets = new HashSet<int>();
            }
            else buckets.Clear();


            if (surroundings.perceptionRadius > 0)
            {
                GetBucketsOverlappingSphere(position, surroundings.perceptionRadius, buckets);
            }
            if (surroundings.lookAheadSeconds > 0)
            {
                GetBucketsOverlappingLine(position, position + velocity * surroundings.lookAheadSeconds, buckets);
            }
            if (surroundings.perceptionShapes.Count > 0)
            {
                foreach (System.Tuple<Shape, Vector3> s in surroundings.perceptionShapes)
                {
                    GetBucketsOverlappingSphere(s.Item2, s.Item1.radius, buckets);
                }
            }

            foreach (int bucket in buckets)
            { 
                if(bucketToAgents.TryGetValue(bucket, out _bucketContentsCache))
                {
                    surroundings.AddAgents(_bucketContentsCache);
                }
            }
            if (surroundings.globalSearchTags.Count > 0)
            {
                foreach (string agentTag in surroundings.globalSearchTags) {
                    if (tagToAgents.TryGetValue(agentTag, out _bucketContentsCache))
                    {
                        surroundings.AddAgents(_bucketContentsCache);
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
        public void UpdateAgentBuckets(Agent agent, HashSet<int> buckets, bool isStatic)
        {
            RemoveAgentFromBuckets(agent, buckets);
            AddAgentToBuckets(agent, buckets, isStatic);
        }

        public void RemoveAgentFromBuckets(Agent agent, HashSet<int> buckets)
        {
            if (agentToBuckets.TryGetValue(agent, out buckets))
            {
                foreach(int bucket in buckets)
                {
                    if (bucketToAgents.TryGetValue(bucket, out _bucketContentsCache))
                    {
                        _bucketContentsCache.Remove(agent);
                    }
                }
                agentToBuckets[agent].Clear();
            }

        }


        private HashSet<Agent> _bucketContentsCache;
        private HashSet<int> _bucketListCache;
        private string _tagCache;

        private void AddAgentToBuckets(Agent agent, HashSet<int> buckets, bool isStatic)
        {


            if(lastKnownTag.TryGetValue(agent, out _tagCache)) //tag recorded
            {
                //check for changes
                if (!agent.CompareTag(_tagCache))
                {
                    if (tagToAgents.TryGetValue(_tagCache, out _bucketContentsCache)) //remove from old tag list
                    {
                        _bucketContentsCache.Remove(agent);
                    }
                    _tagCache = agent.tag;
                    lastKnownTag[agent] = _tagCache; //update last known                 

                    if(tagToAgents.TryGetValue(_tagCache, out _bucketContentsCache)) //add to new tag list
                    {
                        _bucketContentsCache.Add(agent);
                    }
                    else
                    {
                        tagToAgents.Add(_tagCache, new HashSet<Agent>() { agent });
                    }
                }
            }
            else //no tag recorded
            {
                _tagCache = agent.tag;
                lastKnownTag.Add(agent, _tagCache); //save last know tag
                if(tagToAgents.TryGetValue(_tagCache, out _bucketContentsCache)) //add to tag list
                {
                    _bucketContentsCache.Add(agent);
                }
                else
                {
                    tagToAgents.Add(agent.tag, new HashSet<Agent>() { agent});

                }
            }


            if (buckets == null)
            {
                buckets = new HashSet<int>();
            }
            buckets.Clear();

            switch (agent.shape.type)
            {
                case Shape.ShapeType.SPHERE:
                    GetBucketsOverlappingSphere(agent.Position, agent.shape.radius, buckets);
                    break;
                case Shape.ShapeType.POINT:
                    buckets.Add ( GetBucketOverlappingPoint(agent.Position) );
                    break;
                default:
                    buckets.Add( GetBucketOverlappingPoint(agent.Position) );
                    break;
            }

            if (!agentToBuckets.TryGetValue(agent, out _bucketListCache))
            {
                agentToBuckets.Add(agent, new HashSet<int>());
            }

            foreach(int bucket in buckets) { 
                if (bucketToAgents.TryGetValue(bucket, out _bucketContentsCache)) //get bucket if already existing
                {
                    if (!capCellCapacity || isStatic || (_bucketContentsCache.Count < maxCellCapacity))
                    {
                        _bucketContentsCache.Add(agent);
                        agentToBuckets[agent].Add(bucket);
                    }
                }

                else //create bucket, add agent
                {
                    bucketToAgents.Add(bucket, new HashSet<Agent>() { agent});
                    agentToBuckets[agent].Add(bucket);

                }
            }


        }

        public int GetBucketOverlappingPoint(Vector3 point)
        {
            return WorldPositionToHash(point);
        }

        public void GetBucketsOverlappingLine(Vector3 start, Vector3 end, HashSet<int> buckets)
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
            int dm = Mathf.Max(Mathf.Max(dx, dy), dz);
            int i = dm;
            x1 = y1 = z1 = dm / 2; /* error offset */

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

        
        public void GetBucketsOverlappingCylinder(Vector3 a, Vector3 b, float r, HashSet<int> buckets)
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

        public void GetBucketsOverlappingSphere(Vector3 center, float radius, HashSet<int> buckets)
        {
            int neighborhoodRadius = 1 + (int)((radius - .01f) / cellSize);
            if(buckets == null) buckets = new HashSet<int>();

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

        public bool ValidatePosition(ref Vector3 position)
        {
            bool valid = true;
            if (dimensions_x <= 0)
            {
                valid = valid && position.x == 0;
                position.x = 0;
            }
            else
            {
                if (position.x < 0)
                {
                    position.x = wrapEdges ? dimensions_x * cellSize + position.x : 0;
                    valid = false;
                }
                else if (position.x > dimensions_x * cellSize)
                {
                    position.x = wrapEdges ? position.x % (dimensions_x * cellSize) : dimensions_x * cellSize;
                    valid = false;
                }
            }
            if (dimensions_y <= 0)
            {
                valid = valid && position.y == 0;
                position.y = 0;
            }
            else
            {
                if (position.y < 0)
                {
                    position.y = wrapEdges ? dimensions_y * cellSize + position.y : 0;
                    valid = false;

                }
                else if (position.y > dimensions_y * cellSize)
                {
                    position.y = wrapEdges ? position.y % (dimensions_y * cellSize) : dimensions_y * cellSize;
                    valid = false;

                }
            }

            if (dimensions_z <= 0)
            {
                valid = valid && position.z == 0;
                position.z = 0;
            }
            else { 
                if (position.z < 0)
                {
                    position.z = wrapEdges ? dimensions_z * cellSize + position.z : 0;
                    valid = false;

                }
                else if (position.z > dimensions_z * cellSize)
                {
                    position.z = wrapEdges ? position.z % (dimensions_z * cellSize) : dimensions_z * cellSize;
                    valid = false;

                }
            }
            return valid;
        }

        public bool ValidateVelocity(ref Vector3 velocity)
        {
            bool valid = true;
            if (dimensions_x <= 0)
            {
                valid = valid && velocity.x == 0;
                velocity.x = 0;
            }
            if (dimensions_y <= 0)
            {
                valid = valid && velocity.y == 0;
                velocity.y = 0;
            }
            if (dimensions_z <= 0)
            {
                valid = valid && velocity.z == 0;
                velocity.z = 0;
            }
            return valid;
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

        private List<int> bucketsToDebugDraw = new List<int>(); //useful for debugging

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

                        if (bucketsToDebugDraw.Contains(bucket))
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
