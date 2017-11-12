using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;

[CustomEditor(typeof(Boid))]
public class BoidEditor : Editor
{
    Boid myBoid;
    bool weightsFoldout = true;
    int moduleSelection = 0;

    List<Type> allModuleTypes;

    void OnEnable()
    {
        myBoid = (Boid)target;
        FindBoidModuleImplementations();
    }


    public override void OnInspectorGUI()
    {

        myBoid.maxSpeed = EditorGUILayout.FloatField("Max Speed", myBoid.maxSpeed);
        weightsFoldout = EditorGUILayout.Foldout(weightsFoldout, "Base Weights");
        if (weightsFoldout) {
            EditorGUILayout.BeginVertical() ;
            myBoid.separationWeight = EditorGUILayout.Slider("Separation", myBoid.separationWeight, 0, 10);
            myBoid.alignmentWeight = EditorGUILayout.Slider("Alignment",myBoid.alignmentWeight, 0, 10);
            myBoid.cohesionWeight = EditorGUILayout.Slider("Cohesion", myBoid.cohesionWeight, 0, 10);
            myBoid.avoidanceWeight = EditorGUILayout.Slider("Avoidance", myBoid.avoidanceWeight, 0, 10);
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Modules");
        List<Type> addedModuleTypes = myBoid.GetSelectedModuleTypes();

        List<Type> toRemove = new List<Type>();
        foreach (Type modType in addedModuleTypes)
        {
            if (!allModuleTypes.Contains(modType))
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
        foreach(Type remType in toRemove) { myBoid.RemoveModuleSelection(remType); }

        List<Type> unaddedModuleTypes = new List<Type>();
        string[] addModOptions = new string[allModuleTypes.Count - myBoid.GetSelectedModuleTypes().Count];
        int optIndex = 0;
        foreach (Type modType in allModuleTypes)
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
            }

            moduleSelection = Mathf.Clamp(EditorGUILayout.Popup(moduleSelection, addModOptions), 0, addModOptions.Length - 1);
            EditorGUILayout.EndHorizontal();
        }
    }

    void FindBoidModuleImplementations()
    {
        allModuleTypes = new List<Type>();
        foreach (Type type in
            Assembly.GetAssembly(typeof(BoidModule)).GetTypes()
            .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(BoidModule))))
        {
            allModuleTypes.Add(type);
        }
    }

    void UnaddedModules()
    {
    }


}
