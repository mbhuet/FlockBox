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

    public List<SteeringBehavior> behaviors;

    private void Awake()
    {
        GetActiveBehaviors();
    }
    private void OnEnable()
    {
        GetActiveBehaviors();
    }



    private List<SteeringBehavior> m_allBehaviors;
    public List<SteeringBehavior> allBehaviors
    {
        get
        {
            //return behaviors;
            if (m_allBehaviors == null) { GetAllBehaviors(); }
            return m_allBehaviors;
        }
    }

    private List<SteeringBehavior> m_activeBehaviors;
    public List<SteeringBehavior> activeBehaviors
    {
        get
        {
            if (m_activeBehaviors == null) { GetActiveBehaviors(); }
            return m_activeBehaviors;
        }
    }

    public void AddBehavior(Type behaviorType)
    {
        
        SteeringBehavior newBehavior = (SteeringBehavior)ScriptableObject.CreateInstance(behaviorType);
        newBehavior.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.NotEditable;

        AssetDatabase.AddObjectToAsset(newBehavior, AssetDatabase.GetAssetPath(this));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();


        behaviors.Add(newBehavior);

    }

    public void RemoveBehavior(SteeringBehavior behavior)
    {
        AssetDatabase.RemoveObjectFromAsset(behavior);
        AssetDatabase.Refresh();
        behaviors.Remove(behavior);
    }

    public void ClearBehaviors()
    {
        foreach(SteeringBehavior behavior in behaviors)
        {
            AssetDatabase.RemoveObjectFromAsset(behavior);
        }
        AssetDatabase.Refresh();


        behaviors.Clear();
    }

    private void GetAllBehaviors()
    {
        m_allBehaviors = new List<SteeringBehavior>();
        FieldInfo[] fields = this.GetType().GetFields();
        foreach (FieldInfo field in fields)
        {
            //Debug.Log(field.Name + " " + field.FieldType.ToString());
            if (field.FieldType.IsSubclassOf(typeof(SteeringBehavior)))
            {
                //Debug.Log("is behavior");
                SteeringBehavior behavior = (SteeringBehavior)field.GetValue(this);
                behavior.OnActiveStatusChange += GetActiveBehaviors;
                m_allBehaviors.Add(behavior);
            }
        }
    }

    private void GetActiveBehaviors()
    {
        m_activeBehaviors = new List<SteeringBehavior>();
        perceptionDistance = 0;
        foreach (SteeringBehavior behavior in allBehaviors)
        {
            if (behavior.isActive)
            {
                m_activeBehaviors.Add(behavior);
                perceptionDistance = Mathf.Max(perceptionDistance, behavior.effectiveRadius);
            }
        }
    }
}
