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
            GenericMenu menu = new GenericMenu();

            foreach (Type type in System.AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(SteeringBehavior)))
            {
                menu.AddItem(new GUIContent(type.ToString()), false, AddBehavior, type);
            }
            menu.ShowAsContext();

        }

        if (GUILayout.Button("Clear Behaviors"))
        {
            targetSettings.ClearBehaviors();
        }
    }

    void AddBehavior(object t)
    {
        targetSettings.AddBehavior((Type)t);
    }
}
