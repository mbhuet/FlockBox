using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;


[CustomEditor(typeof(BehaviorSettings), true)]
public class BehaviorSettingsEditor : Editor
{
    private BehaviorSettings targetSettings;
    private SerializedProperty _behaviors;
    private SerializedProperty _maxForce;
    private SerializedProperty _maxSpeed;

    private List<SteeringBehavior> toRemove = new List<SteeringBehavior>();

    private void OnEnable()
    {
        targetSettings = (BehaviorSettings)target;
        _behaviors = serializedObject.FindProperty("behaviors");
        _maxForce = serializedObject.FindProperty("maxForce");
        _maxSpeed = serializedObject.FindProperty("maxSpeed");
    }
    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(_maxSpeed);
        EditorGUILayout.PropertyField(_maxForce);
        //EditorGUILayout.HelpBox("Create items inside of ItemDatabase.cs", MessageType.Info);



        foreach (SteeringBehavior behavior in targetSettings.behaviors)
        {
            if (behavior.OnInspectorGUI())
            {
                toRemove.Add(behavior);
            }
            GUILayout.Space(10);
        }


        foreach(SteeringBehavior rem in toRemove)
        {
            targetSettings.RemoveBehavior(rem);
        }
        toRemove.Clear();

        GUILayout.BeginVertical("BOX");
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Add Behavior", GUILayout.Width(130)))
        {
            GenericMenu menu = new GenericMenu();

            foreach (Type type in System.AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(SteeringBehavior)))
            {
                menu.AddItem(new GUIContent(type.ToString()), false, AddBehavior, type);
            }
            menu.ShowAsContext();

        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.EndVertical();

        /*
        if (GUILayout.Button("Clear Behaviors"))
        {
            targetSettings.ClearBehaviors();
        }
        */

        serializedObject.ApplyModifiedProperties();
    }

    void AddBehavior(object t)
    {
        targetSettings.AddBehavior((Type)t);
    }
}
