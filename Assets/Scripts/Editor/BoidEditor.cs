using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;

[CustomEditor(typeof(SteeringAgent))]
public class BoidEditor : Editor
{
    SteeringAgent myBoid;
    bool weightsFoldout = true;
    int moduleSelection = 0;

    List<Type> allBehaviorTypes;

    void OnEnable()
    {
        myBoid = (SteeringAgent)target;
        FindBehaviorImplementations();
    }


    public override void OnInspectorGUI()
    {

        myBoid.maxSpeed = EditorGUILayout.FloatField("Max Speed", myBoid.maxSpeed);
        weightsFoldout = EditorGUILayout.Foldout(weightsFoldout, "Base Weights");
        if (weightsFoldout) {
            EditorGUILayout.BeginVertical() ;

            float old_separation = myBoid.separationWeight;
            myBoid.separationWeight = EditorGUILayout.Slider("Separation", myBoid.separationWeight, 0, 10);
            if(myBoid.separationWeight != old_separation) EditorUtility.SetDirty(target);

            float old_alignment = myBoid.alignmentWeight;
            myBoid.alignmentWeight = EditorGUILayout.Slider("Alignment",myBoid.alignmentWeight, 0, 10);
            if(myBoid.alignmentWeight != old_alignment) EditorUtility.SetDirty(target);

            float old_cohesion = myBoid.cohesionWeight;
            myBoid.cohesionWeight = EditorGUILayout.Slider("Cohesion", myBoid.cohesionWeight, 0, 10);
            if(myBoid.cohesionWeight != old_cohesion) EditorUtility.SetDirty(target);

            float old_avoid = myBoid.avoidanceWeight;
            myBoid.avoidanceWeight = EditorGUILayout.Slider("Avoidance", myBoid.avoidanceWeight, 0, 10);
            if(myBoid.avoidanceWeight != old_avoid) EditorUtility.SetDirty(target);

            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Modules");
        List<Type> addedModuleTypes = myBoid.GetSelectedModuleTypes();

        List<Type> toRemove = new List<Type>();
        foreach (Type modType in addedModuleTypes)
        {
            if (!allBehaviorTypes.Contains(modType))
            {
                toRemove.Add(modType);
                continue;
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("-"))
            {
                toRemove.Add(modType);
            }
            EditorGUILayout.LabelField(modType.ToString());
            EditorGUILayout.EndHorizontal();
        }
        foreach(Type remType in toRemove) { myBoid.RemoveModuleSelection(remType);
            EditorUtility.SetDirty(target);
        }

        List<Type> unaddedModuleTypes = new List<Type>();
        string[] addModOptions = new string[allBehaviorTypes.Count - myBoid.GetSelectedModuleTypes().Count];
        int optIndex = 0;
        foreach (Type modType in allBehaviorTypes)
        {
            if (!addedModuleTypes.Contains(modType))
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
                myBoid.AddModuleSelection(unaddedModuleTypes[moduleSelection]);
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
