/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;


[CustomEditor(typeof(BehaviorSettings))]
public class BehaviorSettingsEditor : Editor
{
    List<Type> allBehaviorTypes;

    BehaviorSettings mySettings;
    bool weightsFoldout = true;
    int moduleSelection = 0;

    private SerializedProperty maxSpeed;
    private SerializedProperty maxForce;
    private SerializedProperty behaviors;

    void OnEnable()
    {
        mySettings = (BehaviorSettings)target;
        FindBehaviorImplementations();

        maxSpeed = serializedObject.FindProperty("maxSpeed");
        maxForce = serializedObject.FindProperty("maxForce");
        behaviors = serializedObject.FindProperty("behaviors");
    }


    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();

        EditorGUILayout.PropertyField(maxSpeed, new GUIContent("Max Speed"));
        EditorGUILayout.PropertyField(maxForce, new GUIContent("Max Steering Force"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Behaviors");

        for (int i = 0; i< behaviors.arraySize; i++)
        {
            EditorGUILayout.LabelField(behaviors.GetArrayElementAtIndex(i).CountInProperty().ToString());// FindPropertyRelative("weight").ToString());
        }

        string[] addModOptions = new string[allBehaviorTypes.Count];
        for (int i = 0; i < allBehaviorTypes.Count; i++)
        {
            addModOptions[i] = allBehaviorTypes[i].ToString();
        }

        if (addModOptions.Length > 0)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+", GUILayout.MaxWidth(20)))
            {
                int insertIndex = behaviors.arraySize;
                behaviors.InsertArrayElementAtIndex(insertIndex);
                SerializedProperty p = behaviors.GetArrayElementAtIndex(insertIndex);
                BehaviorInfo info = new BehaviorInfo(allBehaviorTypes[0], 1, 1);
                p.objectReferenceValue = info;
            }

            moduleSelection = Mathf.Clamp(EditorGUILayout.Popup(moduleSelection, addModOptions), 0, addModOptions.Length - 1);
            EditorGUILayout.EndHorizontal();
        }

        /*
        List<BehaviorInfo> toRemove = new List<BehaviorInfo>();
        foreach (BehaviorInfo info in mySettings.behaviors)
        {
            if (!allBehaviorTypes.Contains(info.behaviorType))
            {
                Debug.Log("allBehaviorTypes does not contain " + info.behaviorType.ToString());
                toRemove.Add(info);
                continue;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
            {
                Debug.Log("x click");
                toRemove.Add(info);
            }
            EditorGUILayout.LabelField(info.behaviorType.ToString());
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel++;

            float weight = EditorGUILayout.Slider("Weight", info.weight, 0, 10);
            float radius = EditorGUILayout.FloatField("Effective Radius", info.effectiveRadius);
            if (weight != info.weight || radius != info.effectiveRadius)
            {
                //mySettings.SetOptionsForBehaviorType(behaviorType, new BehaviorOptions(weight, radius));
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Separator();

        }
        foreach (BehaviorInfo remInfo in toRemove) {
            int remIndex = mySettings.IndexOfSelectedBehaviorType(remInfo.behaviorType);
            Debug.Log(remIndex);
            if (remIndex > 0) behaviors.DeleteArrayElementAtIndex(remIndex);
        }

        List<Type> unaddedBehaviorTypes = new List<Type>();
        string[] addModOptions = new string[allBehaviorTypes.Count - behaviors.arraySize];
        int optIndex = 0;
        foreach (Type behaviorType in allBehaviorTypes)
        {
            if (!mySettings.HasSelectedBehaviorType(behaviorType))
            {
                unaddedBehaviorTypes.Add(behaviorType);
                addModOptions[optIndex] = behaviorType.ToString();
                optIndex++;
            }
        }
        if (addModOptions.Length > 0)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+", GUILayout.MaxWidth(20)))
            {
                //mySettings.AddSelectedBehavior(new BehaviorInfo(unaddedBehaviorTypes[moduleSelection], 1, 1));
                EditorUtility.SetDirty(target);
            }

            moduleSelection = Mathf.Clamp(EditorGUILayout.Popup(moduleSelection, addModOptions), 0, addModOptions.Length - 1);
            EditorGUILayout.EndHorizontal();
        }
        */
        /*
        serializedObject.ApplyModifiedProperties();

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