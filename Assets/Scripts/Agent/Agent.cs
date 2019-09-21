using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
[RequireComponent(typeof(AgentVisual))]
public abstract class Agent : MonoBehaviour {

    public const float forceFieldDistance = 10; //how close can a Boid be before it hits the force field

    public Vector3 position { get; protected set; }
    public Vector3 velocity { get; protected set; }
    public Vector3 forward { get; protected set; }

    public float radius = 1f;
    public bool useZLayering;
    public bool drawDebug = false;

    protected SurroundingsDefinition myNeighborhood = new SurroundingsDefinition(0,0,0);

    private AgentVisual m_visual;
    public AgentVisual visual
    {
        get
        {
            if (m_visual == null) m_visual = GetComponent<AgentVisual>();
            return m_visual;
        }
    }

    protected static Dictionary<int, Agent> agentRegistry;
    protected static int agentCount_static = 0;
    public int agentID { get; protected set; }
    public bool isRegistered { get; protected set; }

    protected static Dictionary<System.Type, List<Agent>> agentCache;
    protected static Dictionary<System.Type, List<Agent>> activePopulations;


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


    public virtual bool IsStationary()
    {
        return true;
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
        agentCount_static++;
        agentID = agentCount_static;
        if (agentRegistry == null) agentRegistry = new Dictionary<int, Agent>();
        agentRegistry.Add(agentID, this);
        string name = this.name;
        name = name.Replace("(Clone)", "");
        int underscoreIndex = name.IndexOf('_');
        if (underscoreIndex > -1) name = name.Remove(underscoreIndex);
        name += "_" + agentID;
        this.name = name;

    }

    protected void FindNeighborhood()
    {
        if (!isAlive) return;
//        Debug.Log(this.name +" findNeighborhood");
        Coordinates currentNeighborhood = NeighborhoodCoordinator.WorldPosToNeighborhoodCoordinates(position);
        if (currentNeighborhood.row != myNeighborhood.neighborhoodCoords.row || currentNeighborhood.col != myNeighborhood.neighborhoodCoords.col)
        {
            RemoveFromLastNeighborhood();
            //Debug.Log(this.name + " add to neighborhood " + currentNeighborhood);
            AddToNeighborhood(currentNeighborhood);
        }
    }

    protected void AddToNeighborhood(Coordinates coords)
    {
        NeighborhoodCoordinator.AddAgent(this, coords);
        myNeighborhood.neighborhoodCoords.row = coords.row;
        myNeighborhood.neighborhoodCoords.col = coords.col;
    }

    protected void RemoveFromLastNeighborhood()
    {
//        Debug.Log(this.name + " remove from last neighborhood");
        NeighborhoodCoordinator.RemoveAgent(this, myNeighborhood.neighborhoodCoords);
        myNeighborhood.neighborhoodCoords = Coordinates.nowhere;

    }

    public virtual void Kill()
    {
        if (OnKill != null) OnKill.Invoke(this);
        isAlive = false;
        hasSpawned = false;
        visual.Hide();
        numPursuers = 0;
        RemoveFromLastNeighborhood();
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
        this.position = position;
        ForceWrapPosition();
        AddSelfToActivePopulation();

    }

    public virtual void ForceWrapPosition()
    {
        position = NeighborhoodCoordinator.WrapPosition(position);
        transform.position = this.position;
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
        if (agentRegistry.TryGetValue(agentID, out agentOut))
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
            UnityEditor.Handles.DrawWireDisc(this.transform.position, Vector3.forward, radius);
        }
    }
#endif
}
