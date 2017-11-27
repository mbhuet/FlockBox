/*
 * using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;

[CustomPropertyDrawer(typeof(BehaviorInfo))]
class BehaviorDrawer : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        Rect amountRect = new Rect(position.x, position.y, 30, position.height);
        Rect unitRect = new Rect(position.x, position.y + 30, 50, position.height);
        Rect nameRect = new Rect(position.x, position.y + 60, position.width - 90, position.height);

        // Draw fields - passs GUIContent.none to each so they are drawn without labels
        SerializedProperty weight = property.FindPropertyRelative("weight");
        weight.floatValue = EditorGUI.Slider(amountRect, weight.floatValue, 0, 10);
        //EditorGUI.PropertyField(amountRect, , GUIContent.none);
        //EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("effectiveRadius"), GUIContent.none);

        SerializedProperty type = property.FindPropertyRelative("behaviorTypeName");

        string[] addModOptions = FindBehaviorImplementationsArray();
        typeSelection = Mathf.Clamp(typeSelection, 0, addModOptions.Length-1);
        typeSelection = EditorGUI.Popup(nameRect, typeSelection, addModOptions);
        type.stringValue = addModOptions[typeSelection];

//        EditorGUI.PropertyField(nameRect, property.type("behaviorType"), GUIContent.none);


        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }


    List<String> FindBehaviorImplementations()
    {
        List<string> allBehaviorTypes = new List<string>();
        foreach (Type type in
            Assembly.GetAssembly(typeof(SteeringBehavior)).GetTypes()
            .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(SteeringBehavior))))
        {
            allBehaviorTypes.Add(type.ToString());
        }
        return allBehaviorTypes;
    }

    string[] FindBehaviorImplementationsArray()
    {
        return FindBehaviorImplementations().ToArray();
    }

    int IndexOfTypeName(string type)
    {
        return FindBehaviorImplementations().IndexOf(type);
    }
}
*/