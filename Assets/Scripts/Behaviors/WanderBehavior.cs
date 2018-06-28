using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vexe.Runtime.Types;


[System.Serializable]
public class WanderBehavior : SteeringBehavior {
    [VisibleWhen("isActive"), Range(0,360)]
    public float wanderScope = 90;

    public override Vector3 GetSteeringBehaviorVector(SteeringAgent mine, SurroundingsInfo surroundings)
    {

        Vector3 lastForward = mine.forward.normalized;
        float wanderRotation = (Mathf.PerlinNoise(Time.time, 0) -.5f) * wanderScope;
        //wanderRotation *= Mathf.Deg2Rad;
        Vector3 wanderVector = Quaternion.AngleAxis(wanderRotation, Vector3.forward) * lastForward;

        return wanderVector * mine.activeSettings.maxForce * weight;
    }



    
}
