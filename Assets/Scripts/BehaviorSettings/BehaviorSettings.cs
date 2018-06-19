using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using Vexe.Runtime.Types;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class BehaviorSettings : BaseScriptableObject {
    public float maxForce = 10;    // Maximum steering force
    public float maxSpeed = 2;    // Maximum speed
                                  // Use this for initialization

    private void Awake()
    {
        GetActiveBehaviors();
    }
    private void OnEnable()
    {
        GetActiveBehaviors();
    }

/*
#if UNITY_EDITOR
    [MenuItem("Assets/Create/Behavior Settings")]
    public static void CreateMyAsset()
    {
        BehaviorSettings asset = ScriptableObject.CreateInstance<BehaviorSettings>();
        AssetDatabase.CreateAsset(asset, "Assets/NewBehaviorSettings.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
#endif
*/


    private List<SteeringBehavior> m_allBehaviors;
    public List<SteeringBehavior> allBehaviors
    {
        get
        {
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
        foreach (SteeringBehavior behavior in allBehaviors)
        {
            if (behavior.isActive)
                m_activeBehaviors.Add(behavior);
        }
    }
}
