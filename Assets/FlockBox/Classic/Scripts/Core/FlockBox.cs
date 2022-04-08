using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

#if FLOCKBOX_DOTS
using Unity.Entities;
using Unity.Transforms;
using CloudFine.FlockBox.DOTS;
#endif

namespace CloudFine.FlockBox
{
    public class FlockBox : MonoBehaviour
    {
        public static Action<FlockBox> OnValuesModified;

        private Dictionary<int, HashSet<Agent>> cellToAgents = new Dictionary<int, HashSet<Agent>>(); //get all agents in a cell
        private Dictionary<Agent, HashSet<int>> agentToCells = new Dictionary<Agent, HashSet<int>>(); //get all cells an agent is in

        private List<Agent> allAgents = new List<Agent>();

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

        public Vector3Int Dimensions
        {
            get { return new Vector3Int(DimensionX, DimensionY, DimensionZ); }
            set { DimensionX = value.x; DimensionY = value.y; DimensionZ = value.z; }
        }

        public Vector3 WorldDimensions
        {
            get { return (Vector3)Dimensions * cellSize; }
        }
        public int DimensionX
        {
            get { return dimensions_x; }
            set { dimensions_x = value; }
        }
        public int DimensionY
        {
            get { return dimensions_y; }
            set { dimensions_y = value; }
        }
        public int DimensionZ
        {
            get { return dimensions_z; }
            set { dimensions_z = value; }
        }
        public float CellSize
        {
            get { return cellSize; }
        }
        public int TotalCells
        {
            get { return (Mathf.Max(DimensionX, 1) * Mathf.Max(DimensionY, 1) * Mathf.Max(DimensionZ, 1)); }
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
        public bool useWorldSpace = false;
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
        private bool drawBoundary = true;
        [SerializeField]
        private bool drawOccupiedCells = false;

        private Matrix4x4 _lastLTW = Matrix4x4.identity;

#if FLOCKBOX_DOTS
        [SerializeField]
        private bool useDOTS = false;
        public bool DOTSEnabled
        {
            get { return useDOTS; }
        }

        private Entity agentEntityPrefab;
        public Entity syncedEntityTransform
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

        private EntityManager entityManager
        {
            get
            {
                if(_entityManager == default)
                {
                    _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                }
                return _entityManager;
            }
        }
        private EntityManager _entityManager;


        #region DOTS

        private Entity CreateSyncedRoot()
        {
            //
            // Set up root entity that will follow FlockBox GameObject
            //
            Entity root = entityManager.CreateEntity(new ComponentType[]{
                        typeof(LocalToWorld),
                    });

            entityManager.AddComponentObject(root, this.transform);
            entityManager.AddComponentData<CopyTransformFromGameObject>(root, new CopyTransformFromGameObject { });
            return root;
        }

        public void ConvertGameObjectsToEntities(Agent[] agents)
        {
            var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, new BlobAssetStore());
            for(int i =0; i<agents.Length; i++)
            {
                agents[i].Spawn(this); //Spawn will set Position
                Entity e = GameObjectConversionUtility.ConvertGameObjectHierarchy(agents[i].gameObject, settings);
                SetupEntity(e);
                GameObject.Destroy(agents[i].gameObject);
            }
            settings.BlobAssetStore.Dispose();
        }

        private void SetupEntity(Entity entity)
        {
            entityManager.AddComponentData<FlockMatrixData>(entity, new FlockMatrixData { WorldToFlockMatrix = transform.worldToLocalMatrix });
            entityManager.AddSharedComponentData<FlockData>(entity, new FlockData { Flock = this });
            entityManager.AddComponentData<FlockMatrixData>(entity, new FlockMatrixData {WorldToFlockMatrix = transform.worldToLocalMatrix });
            entityManager.AddComponentData<BoundaryData>(entity, new BoundaryData { Dimensions = WorldDimensions, Margin = boundaryBuffer, Wrap = wrapEdges });
        }

        public Entity[] InstantiateAgentEntitiesFromPrefab(Agent prefab, int population)
        {
            var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, new BlobAssetStore());
            agentEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab.gameObject, settings);
            NativeArray<Entity> agents = new NativeArray<Entity>(population, Allocator.Temp);
            entityManager.Instantiate(agentEntityPrefab, agents);

            for (int i = 0; i < population; i++)
            {
                Entity entity = agents[i];
                AgentData agent = entityManager.GetComponentData<AgentData>(entity);
                if (entityManager.HasComponent<SteeringData>(entity))
                {
                    SteeringData steering = entityManager.GetComponentData<SteeringData>(entity);
                    agent.Velocity = UnityEngine.Random.insideUnitSphere * steering.MaxSpeed;
                }
                agent.Position = RandomPosition();
                entityManager.SetComponentData(entity, agent);

                SetupEntity(entity);
            }

            entityManager.DestroyEntity(agentEntityPrefab);
            Entity[] output = agents.ToArray();
            agents.Dispose();
            settings.BlobAssetStore.Dispose();
            return output;
        }


        #endregion
#endif

        void Start()
        {
            _lastLTW = transform.localToWorldMatrix;

            foreach (AgentPopulation pop in startingPopulations)
            {
                if (pop.prefab == null) continue;

#if FLOCKBOX_DOTS
                if (useDOTS)
                {
                    InstantiateAgentEntitiesFromPrefab(pop.prefab, pop.population);
                    ConvertGameObjectsToEntities(GetComponentsInChildren<Agent>());
                }
                else
#endif
                {
                    for(int i =0; i<pop.population; i++)
                    {
                        Agent agent = GameObject.Instantiate(pop.prefab);
                        agent.transform.SetParent(this.transform);
                        agent.Spawn(this, RandomPosition());                     
                    }
                }
            }
        }

        private void Update()
        {
            bool transformChanged = false;

            if (transform.hasChanged)
            {
                MarkAsChanged();
                transformChanged = true;
                transform.hasChanged = false;
            }

            foreach (Agent agent in allAgents)
            {
                if (agent.isActiveAndEnabled)
                {
                    if (useWorldSpace && transformChanged)
                    {
                        agent.CompensateForFlockBoxMovement(_lastLTW);
                    }
                    agent.FlockingUpdate();
                }
            }

            _lastLTW = transform.localToWorldMatrix;

#if UNITY_EDITOR
            //clear here instead of in OnDrawGizmos so that they persist when the editor is paused
            cellsToDebugDraw.Clear();
#endif
        }

        private void LateUpdate()
        {
            foreach (Agent agent in allAgents)
            {
                if (agent.isActiveAndEnabled)
                {
                    agent.FlockingLateUpdate();
                }
            }
        }


        private void OnValidate()
        {
            MarkAsChanged();
        }

        public void MarkAsChanged()
        {
            if (OnValuesModified != null) OnValuesModified.Invoke(this);
        }



        public void GetSurroundings(Vector3 position, Vector3 velocity, HashSet<int> cells, SurroundingsContainer surroundings)
        {

            if (cells==null)
            {
                cells = new HashSet<int>();
            }
            else cells.Clear();


            if (surroundings.perceptionRadius > 0)
            {
                GetCellsOverlappingSphere(position, surroundings.perceptionRadius, cells);
            }
            if (surroundings.lookAheadSeconds > 0)
            {
                GetCellsOverlappingLine(position, position + velocity * surroundings.lookAheadSeconds, cells);
            }
            if (surroundings.perceptionSpheres.Count > 0)
            {
                foreach (System.Tuple<float, Vector3> s in surroundings.perceptionSpheres)
                {
                    GetCellsOverlappingSphere(s.Item2, s.Item1, cells);
                }
            }

            foreach (int cell in cells)
            { 
                if(cellToAgents.TryGetValue(cell, out _cellContentsCache))
                {
                    surroundings.AddAgents(_cellContentsCache);
                }
            }

            if (surroundings.globalSearchTags.Count > 0)
            {
                foreach (string agentTag in surroundings.globalSearchTags) {
                    if (tagToAgents.TryGetValue(agentTag, out _cellContentsCache))
                    {
                        surroundings.AddAgents(_cellContentsCache);
                    }
                }
            }   
        }

        public void RegisterAgentUpdates(Agent agent)
        {
            if (!allAgents.Contains(agent))
            {
                allAgents.Add(agent);
            }
        }

        public void UnregisterAgentUpdates(Agent agent)
        {
            allAgents.Remove(agent);
        }

        /// <summary>
        /// Remove from 
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="cells"></param>
        /// <param name="isStatic">Will this agent be updating its position every frame</param>
        public void UpdateAgentCells(Agent agent, HashSet<int> cells, bool isStatic)
        {
            RemoveAgentFromCells(agent, cells);
            AddAgentToCells(agent, cells, isStatic);
        }

        public void RemoveAgentFromCells(Agent agent, HashSet<int> cells)
        {
            if (agentToCells.TryGetValue(agent, out cells))
            {
                foreach (int cell in cells)
                {
                    if (cellToAgents.TryGetValue(cell, out _cellContentsCache))
                    {
                        _cellContentsCache.Remove(agent);
                    }
                }
                agentToCells[agent].Clear();
            }

            if (lastKnownTag.TryGetValue(agent, out _tagCache)) //tag recorded
            {
                if (tagToAgents.TryGetValue(_tagCache, out _cellContentsCache)) //remove from old tag list
                {
                    _cellContentsCache.Remove(agent);
                }
                lastKnownTag.Remove(agent);
            }
        }

        private HashSet<Agent> _cellContentsCache;
        private HashSet<int> _cellListCache;
        private string _tagCache;

        private void AddAgentToCells(Agent agent, HashSet<int> cells, bool isStatic)
        {
            if(lastKnownTag.TryGetValue(agent, out _tagCache)) //tag recorded
            {
                //check for changes
                if (!agent.CompareTag(_tagCache))
                {
                    if (tagToAgents.TryGetValue(_tagCache, out _cellContentsCache)) //remove from old tag list
                    {
                        _cellContentsCache.Remove(agent);
                    }
                    _tagCache = agent.tag;
                    lastKnownTag[agent] = _tagCache; //update last known                 

                    if(tagToAgents.TryGetValue(_tagCache, out _cellContentsCache)) //add to new tag list
                    {
                        _cellContentsCache.Add(agent);
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
                if(tagToAgents.TryGetValue(_tagCache, out _cellContentsCache)) //add to tag list
                {
                    _cellContentsCache.Add(agent);
                }
                else
                {
                    tagToAgents.Add(agent.tag, new HashSet<Agent>() { agent});

                }
            }


            if (cells == null)
            {
                cells = new HashSet<int>();
            }
            cells.Clear();

            switch (agent.shape.type)
            {
                case Shape.ShapeType.SPHERE:
                    GetCellsOverlappingSphere(agent.Position, agent.shape.radius, cells);
                    break;
                case Shape.ShapeType.POINT:
                    cells.Add ( GetCellOverlappingPoint(agent.Position) );
                    break;
                default:
                    cells.Add( GetCellOverlappingPoint(agent.Position) );
                    break;
            }

            if (!agentToCells.TryGetValue(agent, out _cellListCache))
            {
                agentToCells.Add(agent, new HashSet<int>());
            }

            foreach(int cell in cells) { 
                if (cellToAgents.TryGetValue(cell, out _cellContentsCache)) //get cell if already existing
                {
                    if (!capCellCapacity || isStatic || (_cellContentsCache.Count < maxCellCapacity))
                    {
                        _cellContentsCache.Add(agent);
                        agentToCells[agent].Add(cell);
                    }
                }

                else //create cell, add agent
                {
                    cellToAgents.Add(cell, new HashSet<Agent>() { agent});
                    agentToCells[agent].Add(cell);
                }
            }
        }

        public int GetCellOverlappingPoint(Vector3 point)
        {
            return WorldPositionToHash(point);
        }

        public void GetCellsOverlappingLine(Vector3 start, Vector3 end, HashSet<int> cells)
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

            cells.Add(CellPositionToHash(x0, y0, z0));

            for (; ; )
            {  /* loop */
                
                if (dimensions_x > 0 && (x0 < 0 || x0 >= dimensions_x)) break;
                if (dimensions_y > 0 && (y0 < 0 || y0 >= dimensions_y)) break;
                if (dimensions_z > 0 && (z0 < 0 || z0 >= dimensions_z)) break;
                cells.Add(CellPositionToHash(x0, y0, z0));

                if (i-- == 0) break;

                x1 -= dx; if (x1 < 0) { x1 += dm; x0 += sx; }
                y1 -= dy; if (y1 < 0) { y1 += dm; y0 += sy; }
                z1 -= dz; if (z1 < 0) { z1 += dm; z0 += sz; }
            }
        }
  
        public void GetCellsOverlappingCylinder(Vector3 a, Vector3 b, float r, HashSet<int> cells)
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
                        cells.Add(CellPositionToHash(x, y, z));
                    }
                }
            }
        }

        public void GetCellsOverlappingSphere(Vector3 center, float radius, HashSet<int> cells)
        {
            int neighborhoodRadius = 1 + (int)((radius - .01f) / cellSize);
            if(cells == null) cells = new HashSet<int>();

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
                        cells.Add(CellPositionToHash(x, y, z));
                        
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

        public bool ValidateDirection(ref Vector3 direction)
        {
            bool valid = true;
            if (dimensions_x <= 0)
            {
                valid = valid && direction.x == 0;
                direction.x = 0;
            }
            if (dimensions_y <= 0)
            {
                valid = valid && direction.y == 0;
                direction.y = 0;
            }
            if (dimensions_z <= 0)
            {
                valid = valid && direction.z == 0;
                direction.z = 0;
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

        private List<int> cellsToDebugDraw = new List<int>(); //useful for debugging

        private void OnDrawGizmos()
        {
            if (drawBoundary)
            {
                Gizmos.color = Color.grey;
                Gizmos.matrix = this.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(WorldDimensions/2f, WorldDimensions);
                if (!wrapEdges)
                {
                    Gizmos.color = Color.yellow * .75f;
                    Gizmos.DrawWireCube(WorldDimensions/2f,
                        new Vector3(
                            Mathf.Max(0, WorldDimensions.x - boundaryBuffer * 2f),
                            Mathf.Max(0, WorldDimensions.y - boundaryBuffer * 2f),
                            Mathf.Max(0, WorldDimensions.z - boundaryBuffer * 2f)));
                }
            }
            if (drawOccupiedCells)
            {
                DrawOccupiedCells();
            }

        }

        void DrawOccupiedCells()
        {
            if (cellToAgents == null) return;
            
            Gizmos.color = Color.grey * .1f;


            for (int x = 0; x < (dimensions_x > 0 ? dimensions_x : 1); x++)
            {
                for (int y = 0; y < (dimensions_y > 0 ? dimensions_y : 1); y++)
                {
                    for (int z = 0; z < (dimensions_z > 0 ? dimensions_z : 1); z++)
                    {

                        Vector3 corner = new Vector3(x, y, z) * cellSize;
                        int cell = WorldPositionToHash(corner);

                        if (cellsToDebugDraw.Contains(cell))
                        {
                            Gizmos.color = Color.red * .8f;
                            Gizmos.DrawCube(corner + Vector3.one * (cellSize / 2f), Vector3.one * cellSize);
                        }
                        else if (cellToAgents.ContainsKey(cell) && cellToAgents[cell].Count > 0)
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
