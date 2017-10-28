using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    static List<Boid> allBoids;
    Vector3 position;
    Vector3 velocity;
    Vector3 acceleration;
    float visual_radius = 2.0f;
    float maxforce = 1;    // Maximum steering force
    public float maxSpeed = 2;    // Maximum speed

    Vector2 viewExtents;
    Vector2 camPos;


    void Awake()
    {
        viewExtents = new Vector2(Camera.main.orthographicSize * Camera.main.aspect, Camera.main.orthographicSize);
        camPos = Camera.main.transform.position;

        acceleration = new Vector3(0, 0);

        // This is a new Vector3 method not yet implemented in JS
        // velocity = Vector3.random2D();

        // Leaving the code temporarily this way so that this example runs in JS
        float angle = Random.Range(0,Mathf.PI*2);
        velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));

        position = transform.position; 
    }

    void Update()
    {
        flock(allBoids);
        // Update velocity
        velocity += (acceleration) * Time.deltaTime;
        // Limit speed
        velocity = velocity.normalized * Mathf.Min(velocity.magnitude, maxSpeed * Time.deltaTime);
        Debug.DrawLine(position, position + velocity, Color.yellow);

        position += (velocity);

        // Reset accelertion to 0 each cycle
        acceleration *= 0;

        move();
        borders();

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

    void applyForce(Vector3 force)
    {
        // We could add mass here if we want A = F / M
        acceleration +=(force);
    }

    // We accumulate a new acceleration each time based on three rules
    void flock(List<Boid> boids)
    {
        Vector3 sep = separate(boids);   // Separation
        Vector3 ali = align(boids);      // Alignment
        Vector3 coh = cohesion(boids);   // Cohesion
                                         // Arbitrarily weight these forces
        sep *= (1.5f);
        ali *= (1.0f);
        coh *= (1.0f);

        Debug.DrawRay(position, sep, Color.red);
        Debug.DrawRay(position, ali, Color.green);
        Debug.DrawRay(position, coh, Color.blue);

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
        if (position.x < camPos.x - viewExtents.x - visual_radius) position.x = camPos.x + viewExtents.x + visual_radius;
        if (position.y < camPos.y - viewExtents.y - visual_radius) position.y = camPos.y + viewExtents.y + visual_radius;
        if (position.x > camPos.x + viewExtents.x + visual_radius) position.x = camPos.x - viewExtents.x - visual_radius;
        if (position.y > camPos.y + viewExtents.y + visual_radius) position.y = camPos.y - viewExtents.y - visual_radius;
    }
    

    // Separation
    // Method checks for nearby boids and steers away
    Vector3 separate(List<Boid> boids)
    {
        float desiredseparation = 25.0f;
        Vector3 steer = new Vector3(0, 0, 0);
        int count = 0;
        // For every boid in the system, check if it's too close
        foreach (Boid other in boids)
        {
            float d = Vector3.Distance(position, other.position);
            // If the distance is greater than 0 and less than an arbitrary amount (0 when you are yourself)
            if ((d > 0) && (d < desiredseparation))
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
    Vector3 align(List<Boid> boids)
    {
        float neighbordist = 50;
        Vector3 sum = new Vector3(0, 0);
        int count = 0;
        foreach (Boid other in boids)
        {
            float d = Vector3.Distance(position, other.position);
            if ((d > 0) && (d < neighbordist))
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
    Vector3 cohesion(List<Boid> boids)
    {
        float neighbordist = 50.0f;
        Vector3 sum = new Vector3(0, 0);   // Start with empty vector to accumulate all positions
        int count = 0;
        foreach (Boid other in boids)
        {
            float d = Vector3.Distance(position, other.position);
            if ((d > 0) && (d < neighbordist))
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
