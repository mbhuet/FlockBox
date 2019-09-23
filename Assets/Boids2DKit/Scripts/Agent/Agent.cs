using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
[RequireComponent(typeof(AgentVisual))]
public class Agent : MonoBehaviour {

    public static Dictionary<int, Agent> Registry { get; private set; }
    private static int agentCount_static = 0;
    protected static Dictionary<System.Type, List<Agent>> agentCache;
    protected static Dictionary<System.Type, List<Agent>> activePopulations;
    private static AgentUpdater updater;

    public enum NeighborType
    {
        POINT, //occupy only one point, one neighborhood
        AREA //occupy all neighborhoods within radius
    }
    public BehaviorSettings activeSettings;

    public Vector3 Position { get; protected set; } = Vector3.zero;
    public Vector3 Velocity { get; protected set; } = Vector3.zero;
    public Vector3 Forward { get; protected set; } = Vector3.zero;
    public Vector3 Acceleration { get; protected set; } = Vector3.zero;


    [SerializeField]
    private float _radius = 1f;
    public float Radius => _radius;

    public float Throttle => speedThrottle;
    protected float speedThrottle = 1;

    public bool Frozen => freezePosition;
    private bool freezePosition = false;




    public NeighborType neighborType;
    public bool drawDebug = false;

    protected List<Coordinates> myNeighborhoodCoords = new List<Coordinates>();


    private AgentVisual m_visual;
    public AgentVisual visual
    {
        get
        {
            if (m_visual == null) m_visual = GetComponent<AgentVisual>();
            return m_visual;
        }
    }

   


    public int agentID { get; protected set; }
    public bool isRegistered { get; protected set; }
    public bool isAlive { get; protected set; }
    public bool isCaught { get; protected set; }


    protected bool hasSpawned = false;
    private float spawnTime;
    protected float age { get { return Time.time - spawnTime; } }

    public int maxPursuers = 10;
    protected int numPursuers = 0;

    [SerializeField]
    protected Dictionary<string, object> attributes = new Dictionary<string, object>();
    public object GetAttribute(string name)
    {
        object val;
        if (!attributes.TryGetValue(name, out val))
            return false;
        return val;
    }
    public virtual void SetAttribute(string name, object value)
    {
        if (attributes.ContainsKey(name))
            attributes[name] = value;
        else
        {
            attributes.Add(name, value);
        }
    }
    public void RemoveAttribute(string name)
    {
        attributes.Remove(name);
    }
    public bool HasAttribute(string name)
    {
        return attributes.ContainsKey(name);
    }


    public virtual bool IsStationary
    {
       get { return true; }
    }

    protected void LockPosition(bool isLocked)
    {
        freezePosition = isLocked;
    }


    public void GetSeekVector(out Vector3 steer, Vector3 target)
    {
        // Steering = Desired minus Velocity
        steer = (target - Position).normalized * activeSettings.maxSpeed - Velocity;
        steer = steer.normalized * Mathf.Min(steer.magnitude, activeSettings.maxForce);
    }



    public delegate void AgentEvent(Agent agent);
    public AgentEvent OnCaught;
    public AgentEvent OnCatch;
    public AgentEvent OnKill;
    public AgentEvent OnSpawn;

    protected void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneUnloaded += OnSceneChange;
    }

    protected virtual void LateUpdate()
    {
        if(isAlive && IsStationary && NeighborhoodCoordinator.HasMoved)
        {
            ForceWrapPosition();
        }
    }

    protected void OnSceneChange(Scene before)
    {
        Kill();
    }

    protected void Start()
    {
        if (!isRegistered) RegisterNewAgent();
        if (!hasSpawned) Spawn(transform.position);
    }

    protected void RegisterNewAgent()
    {
        //add to registry
        agentCount_static++;
        agentID = agentCount_static;
        if (Registry == null) Registry = new Dictionary<int, Agent>();
        Registry.Add(agentID, this);
        
        //clean up name
        string name = this.name;
        name = name.Replace("(Clone)", "");
        int underscoreIndex = name.IndexOf('_');
        if (underscoreIndex > -1) name = name.Remove(underscoreIndex);
        name += "_" + agentID;
        this.name = name;

        //create AgentUpdater if there isn't one
        if (updater == null)
        {
            GameObject updaterObj = new GameObject();
            updaterObj.hideFlags = HideFlags.HideInHierarchy;
            updater = updaterObj.AddComponent<AgentUpdater>();
        }
    }


    protected void FindNeighborhood()
    {
        if (!isAlive) return;
        switch (neighborType) {
            case (NeighborType.POINT):
                Coordinates currentNeighborhood = NeighborhoodCoordinator.WorldPosToNeighborhoodCoordinates(Position);
                if (!CurrentlyOccupyingNeighborhood(currentNeighborhood))
                {
                    RemoveFromAllNeighborhoods();
                    AddToNeighborhood(currentNeighborhood);
                }
                break;
            case (NeighborType.AREA):
                RemoveFromAllNeighborhoods();
                myNeighborhoodCoords = NeighborhoodCoordinator.AddAreaToNeighborhoods(this);

                break;
        }
    }

    protected bool CurrentlyOccupyingNeighborhood(Coordinates coords)
    {
        return myNeighborhoodCoords.Contains(coords);
    }

    protected void AddToNeighborhood(Coordinates coords)
    {
        NeighborhoodCoordinator.AddAgent(this, coords);
        myNeighborhoodCoords.Add(coords);
    }

    protected void RemoveFromAllNeighborhoods()
    {
        foreach(Coordinates coords in myNeighborhoodCoords)
        {
            NeighborhoodCoordinator.RemoveAgent(this, coords);
        }
        myNeighborhoodCoords.Clear();
    }


    protected void RemoveFromNeighborhood(Coordinates coords)
    {
        NeighborhoodCoordinator.RemoveAgent(this, coords);
        myNeighborhoodCoords.Remove(coords);
    }

    public virtual void Kill()
    {
        if (OnKill != null) OnKill.Invoke(this);
        isAlive = false;
        hasSpawned = false;
        visual.Hide();
        numPursuers = 0;
        RemoveFromAllNeighborhoods();
        RemoveSelfFromActivePopulation();
        AddSelfToCache();
    }

    public virtual void Spawn(Vector3 position, params string[] args)
    {
        Spawn(position);
    }

    public virtual void Spawn(Vector3 position)
    {
        if (OnSpawn != null) OnSpawn.Invoke(this);
        spawnTime = Time.time;
        isAlive = true;
        hasSpawned = true;
        isCaught = false;
        numPursuers = 0;
        visual.Show();
        this.Position = position;
        ForceWrapPosition();
        AddSelfToActivePopulation();

    }

    public virtual void ForceWrapPosition()
    {
        Position = NeighborhoodCoordinator.WrapPosition(Position);
        transform.position = this.Position;
        FindNeighborhood();
    }

    public virtual void CatchAgent(Agent other)
    {
        if (OnCatch != null) OnCatch.Invoke(this);
        other.CaughtBy(this);
    }


    public virtual void CaughtBy(Agent other)
    {
        isCaught = true;
        if (OnCaught != null) OnCaught.Invoke(this);
    }

    public virtual bool CanBePursuedBy(Agent agent)
    {
        int agentTargetID = (int)agent.GetAttribute(PursuitBehavior.targetIDAttributeName);
        return isAlive && !isCaught && (numPursuers < maxPursuers || agentTargetID == agentID);
    }


    public static void InformOfPursuit(bool isBeingPursued, Agent agent, int agentID)
    {
        Agent agentOut;
        if (Registry.TryGetValue(agentID, out agentOut))
        {
            agentOut.InformOfPursuit(isBeingPursued, agent);
        }
    }

    public void InformOfPursuit(bool isBeingPursued, Agent agent)
    {
        if (isBeingPursued) numPursuers++;
        else numPursuers--;
        if (numPursuers < 0) numPursuers = 0;
    }


    private void AddSelfToCache()
    {
        if (agentCache == null) agentCache = new Dictionary<System.Type, List<Agent>>();
        System.Type myType = this.GetType();
        if (!agentCache.ContainsKey(myType))
        {
            agentCache.Add(myType, new List<Agent>());
        }

        if(!agentCache[myType].Contains(this)) agentCache[myType].Add(this);
        this.gameObject.SetActive(false);
    }

    protected void AddSelfToActivePopulation()
    {
        System.Type myType = this.GetType();
        if (activePopulations == null) activePopulations = new Dictionary<System.Type, List<Agent>>();
        if (!activePopulations.ContainsKey(myType))
        {
            activePopulations.Add(myType, new List<Agent>());
        }
        if (!activePopulations[myType].Contains(this)) activePopulations[myType].Add(this);
    }

    protected void RemoveSelfFromActivePopulation()
    {
        System.Type myType = this.GetType();
        if (activePopulations == null) activePopulations = new Dictionary<System.Type, List<Agent>>();
        if (!activePopulations.ContainsKey(myType))
        {
            activePopulations.Add(myType, new List<Agent>());
        }
        activePopulations[myType].Remove(this);
    }

    protected int GetPopulationOfType(System.Type type)
    {
        if (activePopulations == null) activePopulations = new Dictionary<System.Type, List<Agent>>();
        if (!activePopulations.ContainsKey(type))
        {
            return 0;
        }
        return activePopulations[type].Count;
    }



    public Agent GetInstance()
    {
        if (agentCache == null) agentCache = new Dictionary<System.Type, List<Agent>>();
        System.Type myType = this.GetType();
        
        if (!agentCache.ContainsKey(myType))
        {
            agentCache.Add(myType, new List<Agent>());
        }
        List<Agent> cachedAgents = agentCache[myType];
        if (cachedAgents.Count == 0)
        {
            return GameObject.Instantiate(this) as Agent;
        }
        else{
                Agent cachedAgent = cachedAgents[0];
                cachedAgents.RemoveAt(0);
            cachedAgent.gameObject.SetActive(true);
                return cachedAgent;
            }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (drawDebug)
        {
            UnityEditor.Handles.color = Color.grey;
            UnityEditor.Handles.DrawWireDisc(this.transform.position, Vector3.forward, Radius);
        }
    }
#endif
}
