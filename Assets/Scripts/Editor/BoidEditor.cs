using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;

/*
[CustomEditor(typeof(SteeringAgent))]
public class SteeringAgentEditor : Editor
{
    SteeringAgent myAgent;
    bool weightsFoldout = true;
    int moduleSelection = 0;

    List<Type> allBehaviorTypes;

    void OnEnable()
    {
        myAgent = (SteeringAgent)target;
        FindBehaviorImplementations();
    }


    public override void OnInspectorGUI()
    {

        myAgent.maxSpeed = EditorGUILayout.FloatField("Max Speed", myAgent.maxSpeed);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Modules");
        List<Type> addedBehaviorTypes = myAgent.GetSelectedBehaviorTypes();

        List<Type> toRemove = new List<Type>();
        foreach (Type behaviorType in addedBehaviorTypes)
        {
            if (!allBehaviorTypes.Contains(behaviorType))
            {
                toRemove.Add(behaviorType);
                continue;
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("-"))
            {
                toRemove.Add(behaviorType);
            }
            EditorGUILayout.LabelField(behaviorType.ToString());
            EditorGUILayout.EndHorizontal();
        }
        foreach(Type remType in toRemove) { myAgent.RemoveModuleSelection(remType);
            EditorUtility.SetDirty(target);
        }

        List<Type> unaddedModuleTypes = new List<Type>();
        string[] addModOptions = new string[allBehaviorTypes.Count - myAgent.GetSelectedBehaviorTypes().Count];
        int optIndex = 0;
        foreach (Type modType in allBehaviorTypes)
        {
            if (!addedBehaviorTypes.Contains(modType))
            {
                unaddedModuleTypes.Add(modType);
                addModOptions[optIndex] = modType.ToString();
                optIndex++;
            }
        }
        if (addModOptions.Length > 0)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                myAgent.AddModuleSelection(unaddedModuleTypes[moduleSelection]);
                EditorUtility.SetDirty(target);
            }

            moduleSelection = Mathf.Clamp(EditorGUILayout.Popup(moduleSelection, addModOptions), 0, addModOptions.Length - 1);
            EditorGUILayout.EndHorizontal();
        }
    }

    void FindBehaviorImplementations()
    {
        allBehaviorTypes = new List<Type>();
        foreach (Type type in
            Assembly.GetAssembly(typeof(SteeringBehavior)).GetTypes()
            .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(SteeringBehavior))))
        {
            allBehaviorTypes.Add(type);
        }
    }

    
}
*/
