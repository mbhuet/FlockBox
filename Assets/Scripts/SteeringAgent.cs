using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Every SteeringAgent uses the same SteeringBehavior instances, there's only one per type and its stored in a static Dictionary
//SteeringBehaviors will never have instance variables
//SteeringAgents have 

[RequireComponent(typeof(SteeringAgentVisual))]
[System.Serializable]
public class SteeringAgent : MonoBehaviour
{
    static bool drawVectors = false;

    Coordinates lastNeighborhood = new Coordinates(0,0);

    public Vector3 position { get; protected set; }
    public Vector3 velocity { get; protected set; }
    public Vector3 acceleration { get; protected set; }
    float visualRadius = 12.0f;
    
    public bool z_layering = true; //will set position z values based on y value;

    public BehaviorSettings settings;

    //Takes a type, returns instance
	protected Dictionary<string, object> attributes = new Dictionary<string, object>();

    SteeringAgentVisual visual;


    protected void Awake()
    {
        visual = GetComponent<SteeringAgentVisual>();
        acceleration = new Vector3(0, 0);

        // This is a new Vector3 method not yet implemented in JS
        // velocity = Vector3.random2D();

        // Leaving the code temporarily this way so that this example runs in JS
        float angle = UnityEngine.Random.Range(0, Mathf.PI * 2);
        velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * settings.maxSpeed;
    }

	protected void Start(){
		position = transform.position;
	}



    protected void Update()
    {
        flock(NeighborhoodCoordinator.GetSurroundings(lastNeighborhood,1));
        velocity += (acceleration) * Time.deltaTime;
        velocity = velocity.normalized * Mathf.Min(velocity.magnitude, settings.maxSpeed);
        if(drawVectors) Debug.DrawLine(position, position + velocity * Time.deltaTime, Color.yellow);

        position += (velocity * Time.deltaTime );

        // Reset accelertion to 0 each cycle
        acceleration *= 0;

        move();
        borders();
    }

    protected void LateUpdate()
    {
        FindNeighborhood();
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


    //if the SteeringBehaviors this agent needs have not been intantiated in the static Dictionary, create them
    

	public object GetAttribute(string name)
    {
        object val;
        if (!attributes.TryGetValue(name, out val))
            return 0;
        return val;
    }

    public void SetAttribute(string name, float val)
    {
        if (attributes.ContainsKey(name))
            attributes[name] = val;
        else
        {
            attributes.Add(name, val);
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
        foreach (SteeringBehavior behavior in settings.activeBehaviors)
            applyForce(behavior.GetSteeringBehaviorVector(this, surroundings));
    }


    public Vector3 seek(Vector3 target)
    {
        Vector3 desired = target - position;  // A vector pointing from the position to the target
        // Scale to maximum speed
        desired = desired.normalized * (settings.maxSpeed);


        // Steering = Desired minus Velocity
        Vector3 steer = desired - velocity;
        steer = steer.normalized * Mathf.Min(steer.magnitude, settings.maxForce);
         // Limit to maximum steering force
        return steer;
    }

    void move()
    {
        this.transform.position = new Vector3(position.x, position.y, (z_layering? position.y : 0));
        if (visual == null) visual = GetComponent<SteeringAgentVisual>();
        visual.SetRotation(Quaternion.identity);
        visual.SetRotation(Quaternion.Euler(0, 0, (Mathf.Atan2(velocity.y, velocity.x) - Mathf.PI * .5f) * Mathf.Rad2Deg));
    }

    // Wraparound
    
    void borders()
    {
        bool wrap = false;
        Vector3 wrappedPosition = position;
        if (position.x < NeighborhoodCoordinator.min.x) { wrappedPosition.x = NeighborhoodCoordinator.max.x + (position.x - NeighborhoodCoordinator.min.x); wrap = true; }
        if (position.y < NeighborhoodCoordinator.min.y) { wrappedPosition.y = NeighborhoodCoordinator.max.y + (position.y - NeighborhoodCoordinator.min.y); wrap = true; }
        if (position.x > NeighborhoodCoordinator.max.x) { wrappedPosition.x = NeighborhoodCoordinator.min.x + (position.x - NeighborhoodCoordinator.max.x); wrap = true; }
        if (position.y > NeighborhoodCoordinator.max.y) { wrappedPosition.y = NeighborhoodCoordinator.min.y + (position.y - NeighborhoodCoordinator.max.y); wrap = true; }
        if(wrap) position = wrappedPosition;
    }
   

    

   
}
