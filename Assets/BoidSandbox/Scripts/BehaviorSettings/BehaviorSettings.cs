using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "BehaviorSettings")]
public class BehaviorSettings : ScriptableObject {
    public float maxForce = 10;    // Maximum steering force
    public float maxSpeed = 15;    // Maximum speed 

    [SerializeField]
    private SteeringBehavior[] behaviors = new SteeringBehavior[0];


    public SteeringBehavior[] Behaviors => behaviors;
    public int NumBehaviors
    {
        get { return behaviors.Length; }
    }

    public float PerceptionDistance
    {
        get
        {
            //TODO optimize this
            return behaviors.Max(x => x.effectiveRadius);
        }
    }


    public SteeringBehavior GetBehavior(int index)
    {
        if (index < 0 || index >= behaviors.Length) return null;
        return behaviors[index];
    }

}
