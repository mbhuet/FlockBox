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

    [SerializeField]
    private List<SteeringBehavior> m_behaviors;
    public List<SteeringBehavior> behaviors
    {
        get
        {
            if (m_behaviors == null) m_behaviors = new List<SteeringBehavior>();
            return m_behaviors;
        }
        private set
        {
            m_behaviors = value;
            RefreshActiveBehaviors();
        }
    }

    private List<SteeringBehavior> m_activeBehaviors;
    public List<SteeringBehavior> activeBehaviors
    {
        get
        {
            if (m_activeBehaviors == null) m_activeBehaviors = new List<SteeringBehavior>();
            bool refresh = false;
            foreach (SteeringBehavior behavior in behaviors)
            {
                if (behavior.activeDirtyFlag)
                {
                    refresh = true;
                    behavior.activeDirtyFlag = false;
                }
            }
            if (refresh) { RefreshActiveBehaviors(); }
            return m_activeBehaviors;
        }
        private set
        {
            m_activeBehaviors = value;
        }
    }


    public void AddBehavior(Type behaviorType)
    {
        
        SteeringBehavior newBehavior = (SteeringBehavior)ScriptableObject.CreateInstance(behaviorType);
        behaviors.Add(newBehavior);

        AssetDatabase.AddObjectToAsset(newBehavior, AssetDatabase.GetAssetPath(this));
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newBehavior));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
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


    private void RefreshActiveBehaviors()
    {
       activeBehaviors = behaviors.Where(x => x.isActive).ToList();
    }
}
