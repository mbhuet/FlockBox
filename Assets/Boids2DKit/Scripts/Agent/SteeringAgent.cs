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
        NeighborhoodCoordinator.GetSurroundings(ref mySurroundings, Position, activeSettings.PerceptionDistance);
        Flock(mySurroundings);

        if (freezePosition) return;

        Velocity += (acceleration) * Time.deltaTime;
        Velocity = Velocity.normalized * Mathf.Min(Velocity.magnitude, activeSettings.maxSpeed * speedThrottle) ;

        Position += (Velocity * Time.deltaTime);
        Position = NeighborhoodCoordinator.WrapPosition(Position);
        acceleration *= 0;

        UpdateTransform();
    }

    protected override void LateUpdate()
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
        foreach (SteeringBehavior behavior in activeSettings.ActiveBehaviors)
        {
            behavior.GetSteeringBehaviorVector(out steer, this, surroundings);
            steer *= behavior.weight;
            if (behavior.drawVectorLine) Debug.DrawRay(Position, steer, behavior.vectorColor);

            ApplyForce(steer);
        }
    }

    public void GetSeekVector(out Vector3 steer, Vector3 target)
    {
        // Steering = Desired minus Velocity
        steer = (target - Position).normalized * activeSettings.maxSpeed - Velocity;
        steer = steer.normalized * Mathf.Min(steer.magnitude, activeSettings.maxForce);
    }

    void UpdateTransform()
    {

        this.transform.position = Position;
        if (Velocity.magnitude > 0) Forward = Velocity.normalized;

            visual.SetRotation(Quaternion.identity);
            visual.SetRotation(Quaternion.Euler(0, 0, (Mathf.Atan2(Forward.y, Forward.x) - Mathf.PI * .5f) * Mathf.Rad2Deg));
        
    }



    public override void Spawn(Vector3 position)
    {
        base.Spawn(position);
        LockPosition(false);
        speedThrottle = 1;
        acceleration = new Vector3(0, 0);
        float forwardAngle = UnityEngine.Random.Range(0, Mathf.PI * 2);
        Velocity = new Vector3(Mathf.Cos(forwardAngle), Mathf.Sin(forwardAngle)) * activeSettings.maxSpeed;
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
