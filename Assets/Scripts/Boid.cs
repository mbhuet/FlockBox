using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class Neighborhood
{
    LinkedList<Boid> neighbors;
    LinkedList<Obstacle> obstacles;
    public Vector2 neighborhoodCenter;
    public Neighborhood()
    {
        neighbors = new LinkedList<Boid>();
        obstacles = new LinkedList<Obstacle>();
    }
    public Neighborhood(Vector2 pos) : this()
    {
        neighborhoodCenter = pos;
    }
    public void AddNeighbor(Boid occupant) {neighbors.AddLast(occupant);}
    public void RemoveNeighbor(Boid neighbor) {neighbors.Remove(neighbor);}
    public void ClearNeighbors(){neighbors.Clear();}
    public bool IsOccupied() { return neighbors.First != null && obstacles.First != null; }
    public LinkedList<Boid> GetNeighbors() { return neighbors; }
    public void AddObstacle(Obstacle obs) { obstacles.AddLast(obs); }
    public LinkedList<Obstacle> GetObstacles() { return obstacles; }

}

public struct SurroundingsInfo
{
    public SurroundingsInfo(LinkedList<Boid> boids, LinkedList<Obstacle> obs) { neighbors = boids; obstacles = obs; }
    public LinkedList<Boid> neighbors;
    public LinkedList<Obstacle> obstacles;
}

public struct Coordinates
{
    public Coordinates(int r, int c){ row = r; col = c;}
    public int col, row;
}

public class BoidWeights : ScriptableObject
{
    public float separationWeight = (1.5f);
    public float alignmentWeight = (0.7f);
    public float cohesionWeight = (1.1f);
    public float avoidanceWeight = 1.0f;
}

[System.Serializable]
public class Boid : MonoBehaviour
{

    static Neighborhood[,] neighborhoods;
    static bool neighborhoodsInitialized = false;
    static List<Boid> allBoids;
    static Vector2 viewExtents;
    static Vector2 camPos;
    static Vector2 neighborhoodSize;
    static int neighborhoodCols;
    static int neighborhoodRows;

    static bool drawVectors = false;

    Coordinates lastNeighborhood = new Coordinates(0,0);

    Vector3 position;
    Vector3 velocity;
    Vector3 acceleration;
    public float visualRadius = 12.0f;
    float maxforce = 10;    // Maximum steering force
    public float maxSpeed = 2;    // Maximum speed


    float desiredseparationDist = 10f; //move away from neighbors within this radius, vector scales with proximity
    float cohesionRadius = 10.0f; //seek midpoint of all neighbors within this radius
    float alignmentRadius = 10f; //align with neighbors within this radius

    [SerializeField]
    public BoidWeights baseWeights;

    public float separationWeight = (1.5f);
    public float alignmentWeight = (0.7f);
    public float cohesionWeight = (1.1f);
    public float avoidanceWeight = 1.0f;

    public SpriteRenderer sprite { get; private set; }

    private List<BoidModule> modules = new List<BoidModule>();
    private Dictionary<Type, BoidModule> moduleDict = new Dictionary<Type, BoidModule>();
    [SerializeField]
    private string[] selectedModuleTypeNames = { };
    

    protected void Awake()
    {
        if (!neighborhoodsInitialized) InitializeNeighborhoods();
        sprite = GetComponent<SpriteRenderer>();
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
        transform.localScale = new Vector3(.5f, 1, 0) * visualRadius;
        //sprite.color = Color.Lerp(Color.white, primaryColor, Random.Range(0f, 1f));
        foreach (Type modType in GetSelectedModuleTypes())
        {
            LoadModule(modType);
        }
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
        if (allBoids == null) allBoids = new List<Boid>();
        
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

        neighborhoodSize = Vector2.one * Mathf.Max(cohesionRadius, alignmentRadius, desiredseparationDist);
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

    public void LoadModule(Type modType)
    {
        BoidModule mod = (BoidModule)Activator.CreateInstance(modType);
        mod.SetOwner(this);
        modules.Add(mod);
        moduleDict.Add(modType, mod);
    }

    public void LoadModule<T>() where T : BoidModule, new()
    {
        BoidModule mod = new T();
        mod.SetOwner(this);
        modules.Add(mod);
        moduleDict.Add(typeof(T), mod);
    }

    public bool HasModuleOfType(Type modType)
    {
        return moduleDict.ContainsKey(modType);
    }

    public bool HasModuleOfType<T>()
    {
        return moduleDict.ContainsKey(typeof(T));
    }

    public T GetModuleOfType<T>() where T : BoidModule
    {
        return (T)moduleDict[typeof(T)];
    }

    public BoidModule GetModuleOfType(Type modType)
    {
        return moduleDict[modType];
    }

    public List<Type> GetSelectedModuleTypes()
    {
        Debug.Log("GetSelectedModuleTypes");
        List<Type> selectedModuleTypes = new List<Type>();
        foreach(string name in selectedModuleTypeNames)
        {
            Debug.Log(name);
          selectedModuleTypes.Add(Type.GetType(name));
        }
        return selectedModuleTypes;
    }

    public void AddModuleSelection(Type modType)
    {
        string[] expandedSelection = new string[selectedModuleTypeNames.Length + 1];
        selectedModuleTypeNames.CopyTo(expandedSelection, 0);
        expandedSelection[expandedSelection.Length - 1] = modType.ToString();
        selectedModuleTypeNames = expandedSelection;
    }

    public void RemoveModuleSelection(Type modType)
    {
        string[] reducedSelection = new string[selectedModuleTypeNames.Length - 1];
        string remName = modType.ToString();
        int modCount = 0;
        foreach (string name in selectedModuleTypeNames)
        {
            if (name != remName)
            {
                if(modCount < reducedSelection.Length) reducedSelection[modCount] = name;
                modCount++;
            }
        }
        if (modCount == reducedSelection.Length) selectedModuleTypeNames = reducedSelection;
    }

    SurroundingsInfo GetSurroundings(int radius)
    {
        LinkedList<Boid> allNeighbors = new LinkedList<Boid>();
        LinkedList<Obstacle> allObstacles = new LinkedList<Obstacle>();

        for (int r = lastNeighborhood.row - radius; r <= lastNeighborhood.row +radius; r++)
        {
            for(int c = lastNeighborhood.col - radius; c<= lastNeighborhood.col +radius; c++)
            {
                if (r >= 0 && r < neighborhoodRows && c >= 0 && c < neighborhoodCols)
                {
                    foreach(Boid neighbor in neighborhoods[r, c].GetNeighbors())
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

   
    private void OnDrawGizmos()
    {
        //TODO find a way for this to only call once
       //DrawNeighborHoods();
    }

    void DrawNeighborHoods()
    {
        for (int r = 0; r < neighborhoods.GetLength(0); r++)
        {
            for(int c = 0; c < neighborhoods.GetLength(1); c++)
            {
                Gizmos.color = Color.red;
                Vector2 neighborhoodPos = neighborhoods[r, c].neighborhoodCenter;
                if (neighborhoods[r, c].GetObstacles().First != null)
                {
                    Gizmos.DrawWireCube(neighborhoodPos, neighborhoodSize);
                }
                else
                {
                    //Gizmos.DrawWireCube(neighborhoodPos, neighborhoodSize);
                }
                
            }
        }
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

    void applyForce(Vector3 force)
    {
        // We could add mass here if we want A = F / M
        acceleration +=(force);
    }

    // We accumulate a new acceleration each time based on three rules
    void flock(SurroundingsInfo surroundings)
    {
        Vector3 sep = separate(surroundings.neighbors);   // Separation
        Vector3 ali = align(surroundings.neighbors);      // Alignment
        Vector3 coh = cohesion(surroundings.neighbors);   // Cohesion
        Vector3 avd = avoidance(surroundings.obstacles); // Avoidance

                                         // Arbitrarily weight these forces
        sep *= (separationWeight);
        ali *= (alignmentWeight);
        coh *= (cohesionWeight);
        avd *= (avoidanceWeight);

        if (drawVectors)
        {
            Debug.DrawRay(position, avd, Color.red);
            Debug.DrawRay(position, ali, Color.green);
            Debug.DrawRay(position, coh, Color.blue);
            Debug.DrawRay(position, sep, Color.black);
        }

        // Add the force vectors to acceleration
        applyForce(sep);
        applyForce(ali);
        applyForce(coh);
        applyForce(avd);
    }

    // Method to update position
    

    // A method that calculates and applies a steering force towards a target
    // STEER = DESIRED MINUS VELOCITY
    Vector3 seek(Vector3 target)
    {
        Vector3 desired =target - position;  // A vector pointing from the position to the target
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
        this.transform.position = position;
        this.transform.rotation = Quaternion.Euler(0, 0, (Mathf.Atan2(velocity.y, velocity.x) - Mathf.PI * .5f) * Mathf.Rad2Deg);
    }

    // Wraparound
    
    void borders()
    {
        if (position.x < camPos.x - viewExtents.x - visualRadius) position.x = camPos.x + viewExtents.x + visualRadius;
        if (position.y < camPos.y - viewExtents.y - visualRadius) position.y = camPos.y + viewExtents.y + visualRadius;
        if (position.x > camPos.x + viewExtents.x + visualRadius) position.x = camPos.x - viewExtents.x - visualRadius;
        if (position.y > camPos.y + viewExtents.y + visualRadius) position.y = camPos.y - viewExtents.y - visualRadius;
    }
    

    // Separation
    // Method checks for nearby boids and steers away
    Vector3 separate(LinkedList<Boid> boids)
    {
        Vector3 steer = Vector3.zero;
        int count = 0;
        // For every boid in the system, check if it's too close
        foreach (Boid other in boids)
        {
            
            float d = Vector3.Distance(position, other.position);
            // If the distance is greater than 0 and less than an arbitrary amount (0 when you are yourself)
            if ((d > 0) && (d < desiredseparationDist))
            {
                float modFactor = 1;
                foreach (BoidModule mod in modules) { modFactor *= mod.GetModFactor(this, other, BoidVector.SEPARATE); }
                // Calculate vector pointing away from neighbor
                Vector3 diff = position - other.position;
                diff.Normalize();
                diff /= (d);        // Weight by distance
                diff *= modFactor;  // Weight by Modules
                steer += (diff);
                count++;            // Keep track of how many
            }
        }
        // Average -- divide by how many
        if (count > 0)
        {
            steer /= ((float)count);
        }

        // As long as the vector is greater than 0
        if (steer.magnitude > 0)
        {
            // First two lines of code below could be condensed with new Vector3 setMag() method
            // Not using this method until Processing.js catches up
            // steer.setMag(maxspeed);

            // Implement Reynolds: Steering = Desired - Velocity
            steer = steer.normalized * (maxSpeed);
            steer -= (velocity);
            steer = steer.normalized * Mathf.Min(steer.magnitude, maxforce);
        }
        return steer;
    }

    // Alignment
    // For every nearby boid in the system, calculate the average velocity
    Vector3 align(LinkedList<Boid> boids)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;
        foreach (Boid other in boids)
        {
            
            float d = Vector3.Distance(position, other.position);
            if ((d > 0) && (d < alignmentRadius))
            {
                float modFactor = 1;
                foreach (BoidModule mod in modules) { modFactor *= mod.GetModFactor(this, other, BoidVector.ALIGN); }
                sum += (other.velocity) * modFactor;
                count++;
            }
        }
        if (count > 0)
        {
            sum /= ((float)count);
            // First two lines of code below could be condensed with new Vector3 setMag() method
            // Not using this method until Processing.js catches up
            // sum.setMag(maxspeed);

            // Implement Reynolds: Steering = Desired - Velocity
            sum.Normalize();
            sum *= (maxSpeed);
            Vector3 steer = sum - velocity;
            steer = steer.normalized * Mathf.Min(steer.magnitude, maxforce);
            return steer;
        }
        else
        {
            return new Vector3(0, 0);
        }
    }

    // Cohesion
    // For the average position (i.e. center) of all nearby boids, calculate steering vector towards that position
    Vector3 cohesion(LinkedList<Boid> boids)
    {
        Vector3 sum = Vector3.zero;   // Start with empty vector to accumulate all positions
        float count = 0;
        foreach (Boid other in boids)
        {
            
            float d = Vector3.Distance(position, other.position);
            if ((d > 0) && (d < cohesionRadius))
            {
                float modFactor = 1;
                foreach (BoidModule mod in modules){ modFactor *= mod.GetModFactor(this, other, BoidVector.COHESION);}
                sum +=(other.position) * modFactor; // Add position
                count+=modFactor; //getting midpoint of weighted positions means dividing total by sum of those weights. Not necessary when getting average of vectors
            }
        }
        if (count > 0)
        {
            sum /= (count);
            return seek(sum);  // Steer towards the position
        }
        else
        {
            return new Vector3(0, 0);
        }
    }

    Vector3 avoidance(LinkedList<Obstacle> obstacles)
    {
        Vector3 steer = Vector3.zero;
        int count = 0;
        foreach (Obstacle obstacle in obstacles)
        {
            
            float dist = Vector3.Distance(position, obstacle.center);
            if (dist < obstacle.radius + Obstacle.forceFieldDistance)
            {
                float modFactor = 1;
                foreach (BoidModule mod in modules)
                {
                    modFactor *= mod.GetModFactor(this, obstacle, BoidVector.AVOID);
                }
                Vector3 away = (position - obstacle.center).normalized;
                float force = Mathf.Clamp01( 1 - (dist - obstacle.radius) / Obstacle.forceFieldDistance);
                force = force * force;
                //Debug.Log(force);

                steer += (away * force) * modFactor;
                count++;
            }
        }

        if (count > 0)
        {
            steer /= (count);
        }

        // As long as the vector is greater than 0
        if (steer.magnitude > 0)
        {
            // First two lines of code below could be condensed with new Vector3 setMag() method
            // Not using this method until Processing.js catches up
            // steer.setMag(maxspeed);

            // Implement Reynolds: Steering = Desired - Velocity
            steer = steer.normalized * (maxSpeed);
            steer -= (velocity);
            steer = steer.normalized * Mathf.Min(steer.magnitude, maxforce);
        }
        return steer;
    }
}
