using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vexe.Runtime.Types;


//Every SteeringAgent uses the same SteeringBehavior instances, there's only one per type and its stored in a static Dictionary
//SteeringBehaviors will never have instance variables
//SteeringAgents have 

[RequireComponent(typeof(SteeringAgentVisual))]
[System.Serializable]
public class SteeringAgent : Agent
{

    public Vector3 velocity { get; protected set; }
    public Vector3 forward { get; protected set; }
    public Vector3 acceleration { get; protected set; }
    public bool isAlive { get; protected set; }

    public float radius = 1f;

    public int agentID { get; protected set; }
    protected static Dictionary<int, SteeringAgent> agentRegistry;
    protected static int agentCount_static = 0;

    protected float velocityThrottle = 1;


    public bool z_layering = true; //will set position z values based on y value;

    public BehaviorSettings activeSettings;

    public delegate void AgentEvent(SteeringAgent agent);
    public AgentEvent OnCaught;
    public AgentEvent OnCatch;
    public AgentEvent OnKill;
    public AgentEvent OnSpawn;

    //Takes a type, returns instance
    [SerializeField]
	protected Dictionary<string, object> attributes = new Dictionary<string, object>();

    private SteeringAgentVisual m_visual;
    public SteeringAgentVisual visual
    {
        get
        {
            if (m_visual == null) m_visual = GetComponent<SteeringAgentVisual>();
            return m_visual;
        }
    }


    protected void Awake()
    {
        acceleration = new Vector3(0, 0);

        // This is a new Vector3 method not yet implemented in JS
        // velocity = Vector3.random2D();

        // Leaving the code temporarily this way so that this example runs in JS
        float angle = UnityEngine.Random.Range(0, Mathf.PI * 2);
        velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * activeSettings.maxSpeed;
        isAlive = true;
    }

	protected void Start(){
        RegisterNewAgent();
		Spawn(transform.position);
	}



    protected virtual void Update()
    {
        if (!isAlive) return;
        flock(NeighborhoodCoordinator.GetSurroundings(lastNeighborhood,1));
        velocity += (acceleration) * Time.deltaTime;
        velocity = velocity.normalized * Mathf.Min(velocity.magnitude, activeSettings.maxSpeed) * velocityThrottle;

        position += (velocity * Time.deltaTime);

        // Reset accelertion to 0 each cycle
        acceleration *= 0;

        move();
        borders();
    }

    protected void LateUpdate()
    {
        if (!isAlive) return;
        FindNeighborhood();
    }

    protected void RegisterNewAgent()
    {
        agentCount_static++;
        agentID = agentCount_static;
        if (agentRegistry == null) agentRegistry = new Dictionary<int, SteeringAgent>();
        agentRegistry.Add(agentID, this);
        this.name += "_" + agentID;
    }

    protected void FindNeighborhood()
    {
        Coordinates currentNeighborhood = NeighborhoodCoordinator.WorldPosToNeighborhoodCoordinates(position);
        if (currentNeighborhood.row != lastNeighborhood.row || currentNeighborhood.col != lastNeighborhood.col)
        {
            NeighborhoodCoordinator.RemoveNeighbor(this, lastNeighborhood);
            NeighborhoodCoordinator.AddNeighbor(this, currentNeighborhood);
            lastNeighborhood.row = currentNeighborhood.row;
            lastNeighborhood.col = currentNeighborhood.col;
        }
    }

    protected void RemoveFromNeighborhood()
    {
        NeighborhoodCoordinator.RemoveNeighbor(this, lastNeighborhood);

    }


    //if the SteeringBehaviors this agent needs have not been intantiated in the static Dictionary, create them


    public object GetAttribute(string name)
    {
        object val;
        if (!attributes.TryGetValue(name, out val))
            return 0;
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

    void applyForce(Vector3 force)
    {
        // We could add mass here if we want A = F / M
        acceleration +=(force);
    }

    // We accumulate a new acceleration each time based on three rules
    void flock(SurroundingsInfo surroundings)
    {
        foreach (SteeringBehavior behavior in activeSettings.activeBehaviors)
        {
            Vector3 steer = (behavior.GetSteeringBehaviorVector(this, surroundings));
            if (behavior.drawVectorLine) Debug.DrawRay(position, steer, behavior.vectorColor);
            applyForce(steer);
        }
    }


    public Vector3 seek(Vector3 target)
    {
        Vector3 desired = target - position;  // A vector pointing from the position to the target
        // Scale to maximum speed
        desired = desired.normalized * (activeSettings.maxSpeed);


        // Steering = Desired minus Velocity
        Vector3 steer = desired - velocity;
        steer = steer.normalized * Mathf.Min(steer.magnitude, activeSettings.maxForce);
         // Limit to maximum steering force
        return steer;
    }

    void move()
    {

        this.transform.position = new Vector3(position.x, position.y, (z_layering? ZLayering.YtoZPosition(position.y) : 0));
        if (velocity.magnitude > 0) forward = velocity.normalized;

            visual.SetRotation(Quaternion.identity);
            visual.SetRotation(Quaternion.Euler(0, 0, (Mathf.Atan2(forward.y, forward.x) - Mathf.PI * .5f) * Mathf.Rad2Deg));
        
    }

    public virtual void Kill()
    {
        if (OnKill != null) OnKill.Invoke(this);
        isAlive = false;
        visual.Hide();
        NeighborhoodCoordinator.RemoveNeighbor(this, lastNeighborhood);

    }

    public virtual void Spawn(Vector3 position)
    {
        if (OnSpawn != null) OnSpawn.Invoke(this);
        isAlive = true;
        visual.Show();
        this.position = position;
    }

    public virtual void CatchAgent(SteeringAgent other)
    {
        if (OnCatch != null) OnCatch.Invoke(this);
        other.CaughtBy(this);
    }

    public virtual void CatchTarget(Target target)
    {
        target.CaughtBy(this);
    }

    public virtual void CaughtBy(SteeringAgent other)
    {
        //Debug.Log("agent caught");
        if (OnCaught != null) OnCaught.Invoke(this);
    }


    // Wraparound
    
    
   

    

   
}
