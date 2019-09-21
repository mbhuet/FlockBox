using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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

    private SurroundingsInfo mySurroundings = new SurroundingsInfo(new LinkedList<AgentWrapped>(), new Dictionary<string, LinkedList<AgentWrapped>>());
    protected virtual void Update()
    {
        if (!isAlive) return;
        NeighborhoodCoordinator.GetSurroundings(ref mySurroundings, myNeighborhood, activeSettings.perceptionDistance);
        Flock(mySurroundings);
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


    void ApplyForce(Vector3 force, SteeringBehavior behavior)
    {
        Debug.Log(force);
        // We could add mass here if we want A = F / M
        acceleration +=(force);
        if (behavior.drawVectorLine) Debug.DrawRay(position, force, behavior.vectorColor);
    }

    // We accumulate a new acceleration each time based on three rules
    //private Vector3 steer = Vector3.zero; //reuse the same Vector3 for optimization,
    void Flock(SurroundingsInfo surroundings)
    {
        foreach (SteeringBehavior behavior in activeSettings.activeBehaviors)
        {
            Vector3 steer = Vector3.zero;
            ApplyForce(behavior.GetSteeringBehaviorVector(ref steer, this, surroundings) * behavior.weight, behavior);
        }
    }

    private Vector3 _steer;

    public Vector3 GetSeekVector(Vector3 target)
    {

        // Steering = Desired minus Velocity
        _steer = ((target - position).normalized * activeSettings.maxSpeed - velocity).normalized ;
        _steer = _steer.normalized * Mathf.Min(_steer.magnitude, activeSettings.maxForce);
         // Limit to maximum steering force
        return _steer;
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
}
