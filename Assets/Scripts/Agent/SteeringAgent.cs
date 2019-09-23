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
        NeighborhoodCoordinator.GetSurroundings(ref mySurroundings, position, activeSettings.perceptionDistance);
        Flock(mySurroundings);

        if (freezePosition) return;

        velocity += (acceleration) * Time.deltaTime;
        velocity = velocity.normalized * Mathf.Min(velocity.magnitude, activeSettings.maxSpeed * speedThrottle) ;

        position += (velocity * Time.deltaTime);
        position = NeighborhoodCoordinator.WrapPosition(position);
        acceleration *= 0;

        UpdateTransform();
    }

    protected void LateUpdate()
    {
        if (!isAlive) return;
        FindNeighborhood();
    }


    void ApplyForce(Vector3 force)
    {
        // We could add mass here if we want A = F / M
        acceleration +=(force);
    }

    private Vector3 steer = Vector3.zero;

    void Flock(SurroundingsInfo surroundings)
    {
        foreach (SteeringBehavior behavior in activeSettings.activeBehaviors)
        {
            behavior.GetSteeringBehaviorVector(out steer, this, surroundings);
            steer *= behavior.weight;
            if (behavior.drawVectorLine) Debug.DrawRay(position, steer, behavior.vectorColor);

            ApplyForce(steer);
        }
    }

    public void GetSeekVector(out Vector3 steer, Vector3 target)
    {
        // Steering = Desired minus Velocity
        steer = (target - position).normalized * activeSettings.maxSpeed - velocity;
        steer = steer.normalized * Mathf.Min(steer.magnitude, activeSettings.maxForce);
    }

    void UpdateTransform()
    {

        this.transform.position = position;
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

    public override bool IsStationary
    {
        get{ return false; }
    }

    protected void LockPosition(bool isLocked)
    {
        freezePosition = isLocked;
    }
}
