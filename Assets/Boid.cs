using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class Neighborhood
{
    LinkedList<Boid> neighbors;
    public Vector2 neighborhoodCenter;
    public Neighborhood()
    {
        neighbors = new LinkedList<Boid>();
    }
    public Neighborhood(Vector2 pos)
    {
        neighbors = new LinkedList<Boid>();
        neighborhoodCenter = pos;
    }
    public void AddNeighbor(Boid occupant) {neighbors.AddLast(occupant);}
    public void RemoveNeighbor(Boid neighbor) {neighbors.Remove(neighbor);}
    public void ClearNeighbors(){neighbors.Clear();}
    public bool IsOccupied() { return neighbors.First != null; }
    public LinkedList<Boid> GetNeighbors() { return neighbors; }

}

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
    static bool drawNeighborhoods = false;

    int lastNeighborhood_row = 0;
    int lastNeighborhood_col = 0;

    Vector3 position;
    Vector3 velocity;
    Vector3 acceleration;
    public float visualRadius = 12.0f;
    float maxforce = 10;    // Maximum steering force
    public float maxSpeed = 2;    // Maximum speed


    float desiredseparationDist = 10f; //move away from neighbors within this radius, vector scales with proximity
    float cohesionRadius = 10.0f; //seek midpoint of all neighbors within this radius
    float alignmentRadius = 10f; //align with neighbors within this radius

    SpriteRenderer sprite;
    public Color primaryColor;


    void Awake()
    {
        if (!neighborhoodsInitialized) InitializeNeighborhoods();
        sprite = GetComponent<SpriteRenderer>();
        acceleration = new Vector3(0, 0);

        // This is a new Vector3 method not yet implemented in JS
        // velocity = Vector3.random2D();

        // Leaving the code temporarily this way so that this example runs in JS
        float angle = Random.Range(0, Mathf.PI * 2);
        velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));

        position = transform.position;
    }

    private void Start()
    {
        transform.localScale = new Vector3(.5f, 1, 0) * visualRadius;
        //sprite.color = Color.Lerp(Color.white, primaryColor, Random.Range(0f, 1f));
    }

    void Update()
    {
        flock(SurroundingNeighbors(1));
        // Update velocity
        velocity += (acceleration) * Time.deltaTime;
        // Limit speed
        velocity = velocity.normalized * Mathf.Min(velocity.magnitude, maxSpeed * Time.deltaTime);
        if(drawVectors) Debug.DrawLine(position, position + velocity, Color.yellow);

        position += (velocity);

        // Reset accelertion to 0 each cycle
        //ColorBasedOnSpeed();

        acceleration *= 0;

        move();
        borders();
    }

    private void LateUpdate()
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

    void FindNeighborhood()
    {
        int newNeighborhood_row = Mathf.FloorToInt((position.y + neighborhoodRows /2f * neighborhoodSize.y)/ neighborhoodSize.y);
        int newNeighborhood_col = Mathf.FloorToInt((position.x + neighborhoodCols /2f * neighborhoodSize.x)/ neighborhoodSize.x);
//        Debug.Log(newNeighborhood_row + ", " + newNeighborhood_col);
        if (newNeighborhood_row != lastNeighborhood_row || newNeighborhood_col != lastNeighborhood_col)
        {
            neighborhoods[lastNeighborhood_row, lastNeighborhood_col].RemoveNeighbor(this);
            neighborhoods[newNeighborhood_row, newNeighborhood_col].AddNeighbor(this);
            lastNeighborhood_row = newNeighborhood_row;
            lastNeighborhood_col = newNeighborhood_col;
        }
    }

    LinkedList<Boid> SurroundingNeighbors(int radius)
    {
        LinkedList<Boid> allNeighbors = new LinkedList<Boid>();
        for(int r = lastNeighborhood_row - radius; r <= lastNeighborhood_row +radius; r++)
        {
            for(int c = lastNeighborhood_col - radius; c<= lastNeighborhood_col +radius; c++)
            {
                if (r >= 0 && r < neighborhoodRows && c >= 0 && c < neighborhoodCols)
                {
                    foreach(Boid neighbor in neighborhoods[r, c].GetNeighbors())
                    {
                        allNeighbors.AddLast(neighbor);
                    }
                }
            }
        }
        return allNeighbors;
    }

    private void OnDrawGizmos()
    {
        if (drawNeighborhoods)
            DrawNeighborHoods();
    }

    void ColorBasedOnSpeed()
    {
        sprite.color = Color.Lerp(Color.white, primaryColor, (acceleration.magnitude / .5f));
    }

    void DrawNeighborHoods()
    {
        for (int r = 0; r < neighborhoods.GetLength(0); r++)
        {
            for(int c = 0; c < neighborhoods.GetLength(1); c++)
            {
                Gizmos.color = Color.red;
                Vector2 neighborhoodPos = neighborhoods[r, c].neighborhoodCenter;
                if (neighborhoods[r, c].IsOccupied())
                {
                    Gizmos.DrawCube(neighborhoodPos, neighborhoodSize);
                }
                else
                {
                    Gizmos.DrawWireCube(neighborhoodPos, neighborhoodSize);
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
    void flock(LinkedList<Boid> boids)
    {
        Vector3 sep = separate(boids);   // Separation
        Vector3 ali = align(boids);      // Alignment
        Vector3 coh = cohesion(boids);   // Cohesion
                                         // Arbitrarily weight these forces
        sep *= (1.5f);
        ali *= (0.7f);
        coh *= (1.1f);

        if (drawVectors)
        {
            Debug.DrawRay(position, sep, Color.red);
            Debug.DrawRay(position, ali, Color.green);
            Debug.DrawRay(position, coh, Color.blue);
        }

        // Add the force vectors to acceleration
        applyForce(sep);
        applyForce(ali);
        applyForce(coh);
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
        steer = steer.normalized * Mathf.Min(steer.magnitude, maxforce * Time.deltaTime);
         // Limit to maximum steering force
        return steer;
    }

    void move()
    {
        // Draw a triangle rotated in the direction of velocity
        //float theta = velocity.heading2D() + radians(90);
        // heading2D() above is now heading() but leaving old syntax until Processing.js catches up

        this.transform.position = position;
        this.transform.rotation = Quaternion.EulerRotation(0, 0, Mathf.Atan2(velocity.y, velocity.x) - Mathf.PI * .5f);

        /*
        fill(200, 100);
        stroke(255);
        pushMatrix();
        translate(position.x, position.y);
        rotate(theta);
        beginShape(TRIANGLES);
        vertex(0, -r * 2);
        vertex(-r, r * 2);
        vertex(r, r * 2);
        endShape();
        popMatrix();
        */
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
        Vector3 steer = new Vector3(0, 0, 0);
        int count = 0;
        // For every boid in the system, check if it's too close
        foreach (Boid other in boids)
        {
            float d = Vector3.Distance(position, other.position);
            // If the distance is greater than 0 and less than an arbitrary amount (0 when you are yourself)
            if ((d > 0) && (d < desiredseparationDist))
            {
                // Calculate vector pointing away from neighbor
                Vector3 diff = position - other.position;
                diff.Normalize();
                diff /= (d);        // Weight by distance
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
            steer = steer.normalized * (maxSpeed * Time.deltaTime);
            steer -= (velocity);
            steer = steer.normalized * Mathf.Min(steer.magnitude, maxforce * Time.deltaTime);
        }
        return steer;
    }

    // Alignment
    // For every nearby boid in the system, calculate the average velocity
    Vector3 align(LinkedList<Boid> boids)
    {
        Vector3 sum = new Vector3(0, 0);
        int count = 0;
        foreach (Boid other in boids)
        {
            float d = Vector3.Distance(position, other.position);
            if ((d > 0) && (d < alignmentRadius))
            {
                sum += (other.velocity);
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
            sum *= (maxSpeed * Time.deltaTime);
            Vector3 steer = sum - velocity;
            steer = steer.normalized * Mathf.Min(steer.magnitude, maxforce * Time.deltaTime);
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
        Vector3 sum = new Vector3(0, 0);   // Start with empty vector to accumulate all positions
        int count = 0;
        foreach (Boid other in boids)
        {
            float d = Vector3.Distance(position, other.position);
            if ((d > 0) && (d < cohesionRadius))
            {
                sum+=(other.position); // Add position
                count++;
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
}
