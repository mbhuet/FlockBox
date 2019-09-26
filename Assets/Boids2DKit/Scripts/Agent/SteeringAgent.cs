using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;


//Every SteeringAgent uses the same SteeringBehavior instances, there's only one per type and its stored in a static Dictionary
//SteeringBehaviors will never have instance variables
//SteeringAgents have 

[System.Serializable]
public class SteeringAgent : Agent
{

    public Vector3 Forward { get; protected set; } = Vector3.zero;
    public Vector3 Acceleration { get; protected set; } = Vector3.zero;

    protected float speedThrottle = 1;

    public BehaviorSettings activeSettings;
    private bool freezePosition = false;

    private float threadStart;
    private bool threadRunning = false;
    private WaitCallback callback;
    //Takes a type, returns instance

    private SurroundingsInfo mySurroundings = new SurroundingsInfo();
    private List<int> neighborBuckets = new List<int>();

    protected void Awake()
    {
        callback = new WaitCallback(ThreadFlock);
    }

    protected virtual void Update()
    {
        
        if (!isAlive) return;
        if (activeSettings == null) return;
        if (freezePosition) return;

        if (!threadRunning)
        {
            Velocity += (Acceleration) * (Time.time-threadStart);
            Velocity = Velocity.normalized * Mathf.Min(Velocity.magnitude, activeSettings.maxSpeed * speedThrottle);
            Acceleration *= 0;
            threadStart = Time.time;
            threadRunning = true;
            NeighborhoodCoordinator.GetSurroundings(ref mySurroundings, Position, Radius, ref neighborBuckets);
            if (activeSettings.useThreading)
            {
                ThreadPool.QueueUserWorkItem(callback, mySurroundings);

            }
            else
            {
                Flock();
            }
        }   


        Position += (Velocity * Time.deltaTime);
        NeighborhoodCoordinator.WrapPosition(ref Position);

        this.transform.position = Position;
        if (Velocity.magnitude > 0)
        {
            Forward = Velocity.normalized;
            transform.rotation = (Quaternion.Euler(0, 0, (Mathf.Atan2(Forward.y, Forward.x) - Mathf.PI * .5f) * Mathf.Rad2Deg));
        }

    }

    private void LateUpdate()
    {
        FindNeighborhood();

    }

    private Vector3 steer = Vector3.zero;

    void ThreadFlock(System.Object obj)
    {
        SurroundingsInfo surroundings = (SurroundingsInfo)obj;
        foreach (SteeringBehavior behavior in activeSettings.Behaviors)
        {
            if (!behavior.IsActive) continue;
            behavior.GetSteeringBehaviorVector(out steer, this, surroundings);
            steer *= behavior.weight;
            //if (behavior.drawVectorLine) Debug.DrawRay(Position, steer, behavior.vectorColor);

            // We could add mass here if we want A = F / M
            Acceleration += (steer);
        }

        threadRunning = false;
    }

    void Flock()
    {
        foreach (SteeringBehavior behavior in activeSettings.Behaviors)
        {
            if (!behavior.IsActive) continue;
            behavior.GetSteeringBehaviorVector(out steer, this, mySurroundings);
            steer *= behavior.weight;
            //if (behavior.drawVectorLine) Debug.DrawRay(Position, steer, behavior.vectorColor);

            // We could add mass here if we want A = F / M
            Acceleration += (steer);
        }
        threadRunning = false;
    }

    public void GetSeekVector(out Vector3 steer, Vector3 target)
    {
        // Steering = Desired minus Velocity
        steer = (target - Position).normalized * activeSettings.maxSpeed - Velocity;
        steer = steer.normalized * Mathf.Min(steer.magnitude, activeSettings.maxForce);
    }

    void UpdateTransform()
    {

        
    }



    public override void Spawn(Vector3 position)
    {
        base.Spawn(position);
        LockPosition(false);
        speedThrottle = 1;
        Acceleration = Vector3.zero;
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
