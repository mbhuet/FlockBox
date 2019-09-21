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

public class BehaviorSettings : ScriptableObject {
    public float maxForce = 10;    // Maximum steering force
    public float maxSpeed = 2;    // Maximum speed

    public float perceptionDistance { get; protected set; }
    public float attention = 1;

    private void Awake()
    {
        RefreshActiveBehaviors();
    }
    private void OnEnable()
    {
        RefreshActiveBehaviors();
    }


    public List<SteeringBehavior> behaviors
    {
        get; private set;
    }


    public List<SteeringBehavior> activeBehaviors
    {
        get; private set;
    }

    public void AddBehavior(Type behaviorType)
    {
        
        SteeringBehavior newBehavior = (SteeringBehavior)ScriptableObject.CreateInstance(behaviorType);
        newBehavior.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.NotEditable;

        AssetDatabase.AddObjectToAsset(newBehavior, AssetDatabase.GetAssetPath(this));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        newBehavior.OnActiveStatusChange += OnBehaviorActiveChange;
        behaviors.Add(newBehavior);
        RefreshActiveBehaviors();
    }



    public void RemoveBehavior(SteeringBehavior behavior)
    {
        AssetDatabase.RemoveObjectFromAsset(behavior);
        AssetDatabase.Refresh();
        behaviors.Remove(behavior);
        RefreshActiveBehaviors();
    }

    public void ClearBehaviors()
    {
        foreach(SteeringBehavior behavior in behaviors)
        {
            AssetDatabase.RemoveObjectFromAsset(behavior);
        }
        AssetDatabase.Refresh();

        behaviors.Clear();
        RefreshActiveBehaviors();
    }

    private void OnBehaviorActiveChange(bool isActive)
    {
        RefreshActiveBehaviors();
    }

    private void RefreshActiveBehaviors()
    {
       activeBehaviors = behaviors.Where(x => x.isActive).ToList();
    }
}
