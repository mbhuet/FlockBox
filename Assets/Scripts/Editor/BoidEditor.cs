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
    string[] moduleStrings;
    List<Type> moduleTypes;
    List<Type> addedModules = new List<Type>();

    void OnEnable()
    {
        myBoid = (Boid)target;
        moduleStrings = GetNamesOfBoidModuleSubclasses();
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Max Speed", GUILayout.MaxWidth(100));
        myBoid.maxSpeed = EditorGUILayout.FloatField(myBoid.maxSpeed);
        EditorGUILayout.EndHorizontal();
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
        foreach (Type modType in addedModules)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(modType.ToString());
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.DropdownButton(new GUIContent("Dropdown", "Tooltip"), FocusType.Keyboard);
        for(int i = 0; i< moduleStrings.Length; i++)
        {
            EditorGUILayout.LabelField(moduleStrings[i]);
        }
    }

    string[] GetNamesOfBoidModuleSubclasses()
    {
        List<Type> moduleTypes = new List<Type>();
        foreach (Type type in
            Assembly.GetAssembly(typeof(BoidModule)).GetTypes()
            .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(BoidModule))))
        {
            moduleTypes.Add(type);
        }
        string[] names = new string[moduleTypes.Count];
        for(int i = 0; i< names.Length; i++)
        {
            names[i] = moduleTypes[i].ToString();
        }

        return names;

    }

    public static IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : BoidModule
    {
        List<T> objects = new List<T>();
        foreach (Type type in
            Assembly.GetAssembly(typeof(T)).GetTypes()
            .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
        {
            objects.Add((T)Activator.CreateInstance(type, constructorArgs));
        }
        objects.Sort();
        return objects;
    }


}
