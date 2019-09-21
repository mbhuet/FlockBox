using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class WanderBehavior : SteeringBehavior {
    public float wanderScope = 90;

    public override Vector3 GetSteeringBehaviorVector(ref Vector3 steer, SteeringAgent mine, SurroundingsInfo surroundings)
    {
        Vector3 lastForward = mine.forward.normalized;
        float wanderRotation = (Mathf.PerlinNoise(Time.time, mine.gameObject.GetInstanceID()) -.5f) * wanderScope;
        //wanderRotation *= Mathf.Deg2Rad;
        Vector3 wanderVector = Quaternion.AngleAxis(wanderRotation, Vector3.forward) * lastForward;

        return steer * mine.activeSettings.maxForce;
    }



    
}
