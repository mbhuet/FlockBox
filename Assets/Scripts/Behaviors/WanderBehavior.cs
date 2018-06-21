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

        Vector3 lastForward = mine.velocity.normalized;
        float wanderRotation = (Mathf.PerlinNoise(Time.time, 0) * 2 - 1) * wanderScope / 2f;
        //wanderRotation *= Mathf.Deg2Rad;
        Vector3 wanderVector = Quaternion.AngleAxis(wanderRotation, Vector3.forward) * lastForward;

        return wanderVector * mine.activeSettings.maxForce * weight;
    }



    
}
