using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vexe.Runtime.Types;


//Every SteeringAgent uses the same SteeringBehavior instances, there's only one per type and its stored in a static Dictionary
//SteeringBehaviors will never have instance variables
//SteeringAgents have 

[System.Serializable]
public class SteeringAgent : Agent
{

    
    public Vector3 acceleration { get; protected set; }

    protected float speedThrottle = 1;

    public BehaviorSettings activeSettings;
    private bool freezePosition = false;
    //Takes a type, returns instance

	
    protected virtual void Update()
    {
        if (!isAlive) return;
        flock(NeighborhoodCoordinator.GetSurroundings(lastNeighborhood, activeSettings.perceptionDistance));
        if (freezePosition) return;
        velocity += (acceleration) * Time.deltaTime;
        velocity = velocity.normalized * Mathf.Min(velocity.magnitude, activeSettings.maxSpeed * speedThrottle) ;
        //Debug.Log(velocity * Time.deltaTime);
        position += (velocity * Time.deltaTime);
        position = NeighborhoodCoordinator.WrapPosition(position);
        // Reset accelertion to 0 each cycle
        acceleration *= 0;

        UpdateTransform();
    }

    protected void LateUpdate()
    {
        if (!isAlive) return;
        FindNeighborhood();
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

    void UpdateTransform()
    {

        this.transform.position = new Vector3(position.x, position.y, (useZLayering? ZLayering.YtoZPosition(position.y) : 0));
        if (velocity.magnitude > 0) forward = velocity.normalized;

            visual.SetRotation(Quaternion.identity);
            visual.SetRotation(Quaternion.Euler(0, 0, (Mathf.Atan2(forward.y, forward.x) - Mathf.PI * .5f) * Mathf.Rad2Deg));
        
    }



    public override void Spawn(Vector3 position)
    {
        base.Spawn(position);
        LockPosition(false);
        speedThrottle = 1;
        acceleration = new Vector3(0, 0);
        float forwardAngle = UnityEngine.Random.Range(0, Mathf.PI * 2);
        velocity = new Vector3(Mathf.Cos(forwardAngle), Mathf.Sin(forwardAngle)) * activeSettings.maxSpeed;
    }

    public override bool IsStationary()
    {
        return false;
    }

    protected void LockPosition(bool isLocked)
    {
        freezePosition = isLocked;
    }


    // Wraparound







}
