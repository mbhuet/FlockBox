using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SteeringBehavior), true)]
public class SteeringBehaviorDrawer : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      
        SteeringBehavior behavior = property.objectReferenceValue as SteeringBehavior;
        SerializedObject serializedObject = new SerializedObject(behavior);

        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        SerializedProperty isActiveProp = serializedObject.FindProperty("isActive");
        EditorGUILayout.PropertyField(isActiveProp);


        if (isActiveProp.boolValue)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            EditorGUILayout.BeginVertical("BOX");
            EditorGUILayout.Slider(serializedObject.FindProperty("weight"), 0f,1f);
            //EditorGUILayout.Toggle()

            EditorGUILayout.PropertyField(serializedObject.FindProperty("effectiveRadius"));

            SerializedProperty useTag = serializedObject.FindProperty("useTagFilter");
            EditorGUILayout.PropertyField(useTag);
            if (useTag.boolValue)
            {
                SerializedProperty tags = serializedObject.FindProperty("filterTags");
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.BeginVertical("BOX", GUILayout.ExpandWidth(true));

                for (int i=0; i<tags.arraySize; i++)
                {
                    GUILayout.BeginHorizontal();

                    tags.GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TagField(tags.GetArrayElementAtIndex(i).stringValue);
                    if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(14)))
                    {
                        behavior.RemoveTag(i);
                    }
                    GUILayout.EndHorizontal();

                }
                if (GUILayout.Button("+ Add Tag", GUILayout.Width(80)))
                {
                    behavior.AddTag();
                }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }



            SerializedProperty drawVectorProp = serializedObject.FindProperty("drawVectorLine");
            EditorGUILayout.PropertyField(drawVectorProp);
            if (drawVectorProp.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                SerializedProperty color = serializedObject.FindProperty("vectorColor");
                color.colorValue = EditorGUILayout.ColorField(color.colorValue);
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(5);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        serializedObject.ApplyModifiedProperties();
    }

}
