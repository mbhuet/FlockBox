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

    public void AddBehavior(Type behaviorType)
    {
        
        SteeringBehavior newBehavior = (SteeringBehavior)ScriptableObject.CreateInstance(behaviorType);
        newBehavior.hideFlags = HideFlags.HideInHierarchy;
        Array.Resize(ref behaviors, behaviors.Length + 1);
        behaviors[behaviors.Length - 1] = newBehavior;
        AssetDatabase.AddObjectToAsset(newBehavior, AssetDatabase.GetAssetPath(this));
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newBehavior));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }



    public void RemoveBehavior(SteeringBehavior behavior)
    { 
        if (behaviors.Length == 0) return;
        int remIndex = Array.IndexOf(behaviors, behavior);
        if (remIndex< 0) return;

        RemoveBehavior(remIndex);
    }

    public void RemoveBehavior(int index)
    {
        AssetDatabase.RemoveObjectFromAsset(behaviors[index]);
        AssetDatabase.Refresh();

        SteeringBehavior[] newBehaviors = new SteeringBehavior[behaviors.Length - 1];
        for (int i = 0; i < behaviors.Length; i++)
        {
            if (i == index) { }
            else if (i > index) { newBehaviors[i - 1] = behaviors[i]; }
            else { newBehaviors[i] = behaviors[i]; }
        }
        behaviors = newBehaviors;
    }

}
