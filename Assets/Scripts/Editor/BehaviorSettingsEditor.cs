using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(BehaviorSettings), true)]
public class BehaviorSettingsEditor : Editor
{
    private BehaviorSettings targetSettings;
    private SerializedProperty _behaviors;

    private void OnEnable()
    {
        targetSettings = (BehaviorSettings)target;
        _behaviors = serializedObject.FindProperty("behaviors");
    }
    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(_behaviors, true);
        if (GUILayout.Button("Add Behavior"))
        {
            Type behaviorType = typeof(CohesionBehavior);
            targetSettings.AddBehavior(behaviorType);
        }

        if (GUILayout.Button("Clear Behaviors"))
        {
            targetSettings.ClearBehaviors();
        }
    }
}
