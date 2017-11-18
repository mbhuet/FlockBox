using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Every behavior needs to have a weight, an effective distance


[System.Serializable]
public class SteeringAgent : MonoBehaviour
{
    static Neighborhood[,] neighborhoods;
    static bool neighborhoodsInitialized = false;
    static List<SteeringAgent> allBoids;
    static Vector2 viewExtents;
    static Vector2 camPos;
    static Vector2 neighborhoodSize;
    static int neighborhoodCols;
    static int neighborhoodRows;

    static bool drawVectors = false;

    Coordinates lastNeighborhood = new Coordinates(0,0);

    public Vector3 position { get; protected set; }
    public Vector3 velocity { get; protected set; }
    public Vector3 acceleration { get; protected set; }
    float visualRadius = 12.0f;
    public float maxforce = 10;    // Maximum steering force
    public float maxSpeed = 2;    // Maximum speed
    public bool z_layering; //will set position z values based on y value;

    public static List<SteeringBehavior> behaviors;
    static bool behaviorsInitialized = false;


    protected Dictionary<string, float> attributes;

    float[] effectiveDistances = { 10f };

    protected void Awake()
    {
        if (!neighborhoodsInitialized) InitializeNeighborhoods();
        if (!behaviorsInitialized) InitializeBehaviors();
        acceleration = new Vector3(0, 0);

        // This is a new Vector3 method not yet implemented in JS
        // velocity = Vector3.random2D();

        // Leaving the code temporarily this way so that this example runs in JS
        float angle = UnityEngine.Random.Range(0, Mathf.PI * 2);
        velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * maxSpeed;

    }

    protected void Start()
    {
        position = transform.position;
    }

    protected void Update()
    {
        flock(GetSurroundings(1));
        // Update velocity
        velocity += (acceleration) * Time.deltaTime;
        // Limit speed 
        velocity = velocity.normalized * Mathf.Min(velocity.magnitude, maxSpeed);
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

    void Register()
    {
        if (allBoids == null) allBoids = new List<SteeringAgent>();
        
        allBoids.Add(this);
    }
    void Unregister()
    {
        allBoids.Remove(this);
    }

    private void OnEnable()
    {
        Register();
    }

    private void OnDisable()
    {
        Unregister();
    }

    void InitializeNeighborhoods()
    {
        viewExtents = new Vector2(Camera.main.orthographicSize * Camera.main.aspect, Camera.main.orthographicSize);
        camPos = Camera.main.transform.position;

        neighborhoodSize = Vector2.one * Mathf.Max(effectiveDistances);
        neighborhoodCols = Mathf.FloorToInt((viewExtents.x + visualRadius) * 2 / neighborhoodSize.x) + 1;
        neighborhoodRows = Mathf.FloorToInt((viewExtents.y + visualRadius) * 2 / neighborhoodSize.y) + 1;
        neighborhoods = new Neighborhood[neighborhoodRows, neighborhoodCols];
        for (int r = 0; r < neighborhoodRows; r++)
        {
            for(int c = 0; c< neighborhoodCols; c++)
            {
                Vector2 neighborhoodPos = camPos + new Vector2(((c - neighborhoodCols / 2f) + .5f) * neighborhoodSize.x, ((r - neighborhoodRows / 2f) + .5f) * neighborhoodSize.y);
                neighborhoods[r, c] = new Neighborhood(neighborhoodPos);

            }
        }

        neighborhoodsInitialized = true;
    }

    protected static Coordinates WorldPosToNeighborhoodCoordinates(Vector2 position)
    {
        Coordinates coords = new Coordinates();
        coords.col = Mathf.FloorToInt((position.x + neighborhoodCols / 2f * neighborhoodSize.x) / neighborhoodSize.x);
        coords.row = Mathf.FloorToInt((position.y + neighborhoodRows / 2f * neighborhoodSize.y) / neighborhoodSize.y);
        return coords;
    }

    protected void FindNeighborhood()
    {
        Coordinates newNeighborhood = WorldPosToNeighborhoodCoordinates(position);
//        Debug.Log(newNeighborhood_row + ", " + newNeighborhood_col);
        if (newNeighborhood.row != lastNeighborhood.row || newNeighborhood.col != lastNeighborhood.col)
        {
            neighborhoods[lastNeighborhood.row, lastNeighborhood.col].RemoveNeighbor(this);
            neighborhoods[newNeighborhood.row, newNeighborhood.col].AddNeighbor(this);
            lastNeighborhood.row = newNeighborhood.row;
            lastNeighborhood.col = newNeighborhood.col;
        }
    }   

    SurroundingsInfo GetSurroundings(int neighborhoodRadius)
    {
        LinkedList<SteeringAgent> allNeighbors = new LinkedList<SteeringAgent>();
        LinkedList<Obstacle> allObstacles = new LinkedList<Obstacle>();

        for (int r = lastNeighborhood.row - neighborhoodRadius; r <= lastNeighborhood.row +neighborhoodRadius; r++)
        {
            for(int c = lastNeighborhood.col - neighborhoodRadius; c<= lastNeighborhood.col +neighborhoodRadius; c++)
            {
                if (r >= 0 && r < neighborhoodRows && c >= 0 && c < neighborhoodCols)
                {
                    foreach(SteeringAgent neighbor in neighborhoods[r, c].GetNeighbors())
                    {
                        allNeighbors.AddLast(neighbor);
                    }
                    foreach (Obstacle obstacle in neighborhoods[r, c].GetObstacles())
                    {
                        if (!allObstacles.Contains(obstacle))
                            allObstacles.AddLast(obstacle);
                    }
                }
            }
        }
        SurroundingsInfo data = new SurroundingsInfo(allNeighbors, allObstacles);
        return data;
    }


    public static void AddObstacleToNeighborhoods(Obstacle obs)
    {
        Coordinates centerCoords = WorldPosToNeighborhoodCoordinates(obs.center);
        int radius = 1 + Mathf.FloorToInt((obs.radius + Obstacle.forceFieldDistance )/ neighborhoodSize.x);
//        Debug.Log((obs.radius + Obstacle.forceFieldDistance) + " " + radius);
        for(int r = centerCoords.row - radius; r<= centerCoords.row + radius; r++)
        {
            for(int c = centerCoords.col -radius; c <= centerCoords.col + radius; c++)
            {
                if (r >= 0 && r < neighborhoodRows && c >= 0 && c < neighborhoodCols)
                {
                    Neighborhood checkNeighborhood = neighborhoods[r, c];
                    // Find the closest point to the circle within the rectangle
                    float closestX = Mathf.Clamp(obs.center.x,
                        checkNeighborhood.neighborhoodCenter.x - neighborhoodSize.x / 2f,
                        checkNeighborhood.neighborhoodCenter.x + neighborhoodSize.x / 2f);
                    float closestY = Mathf.Clamp(obs.center.y,
                        checkNeighborhood.neighborhoodCenter.y - neighborhoodSize.y / 2f,
                        checkNeighborhood.neighborhoodCenter.y + neighborhoodSize.y / 2f);

                    // Calculate the distance between the circle's center and this closest point
                    float distanceX = obs.center.x - closestX;
                    float distanceY = obs.center.y - closestY;

                    // If the distance is less than the circle's radius, an intersection occurs
                    float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
                    if (distanceSquared < ((obs.radius + Obstacle.forceFieldDistance) * (obs.radius + Obstacle.forceFieldDistance)))
                        neighborhoods[r, c].AddObstacle(obs);
                }
            }
        }
    }

    static void InitializeBehaviors()
    {
        behaviors = new List<SteeringBehavior>();
        foreach()
        behaviorsInitialized = true;
    }

    public float GetAttribute(string name)
    {
        float val;
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
        foreach (BehaviorDelegate behavior in behaviors)
        {
            Vector3 steer = behavior(this, surroundings, 10);
        }
    }


    public Vector3 seek(Vector3 target)
    {
        Vector3 desired = target - position;  // A vector pointing from the position to the target
        // Scale to maximum speed
        desired = desired.normalized * (maxSpeed);


        // Steering = Desired minus Velocity
        Vector3 steer = desired - velocity;
        steer = steer.normalized * Mathf.Min(steer.magnitude, maxforce);
         // Limit to maximum steering force
        return steer;
    }

    void move()
    {
        this.transform.position = new Vector3(position.x, position.y, (z_layering? position.y : 0));
        //this.transform.rotation = Quaternion.Euler(0, 0, (Mathf.Atan2(velocity.y, velocity.x) - Mathf.PI * .5f) * Mathf.Rad2Deg);
    }

    // Wraparound
    
    void borders()
    {
        if (position.x < camPos.x - viewExtents.x - visualRadius) position.x = camPos.x + viewExtents.x + visualRadius;
        if (position.y < camPos.y - viewExtents.y - visualRadius) position.y = camPos.y + viewExtents.y + visualRadius;
        if (position.x > camPos.x + viewExtents.x + visualRadius) position.x = camPos.x - viewExtents.x - visualRadius;
        if (position.y > camPos.y + viewExtents.y + visualRadius) position.y = camPos.y - viewExtents.y - visualRadius;
    }
   

    

   
}
