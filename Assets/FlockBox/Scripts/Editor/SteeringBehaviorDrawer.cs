using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace CloudFine
{
    [CustomPropertyDrawer(typeof(SteeringBehavior), true)]
    public class SteeringBehaviorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SteeringBehavior behavior = property.objectReferenceValue as SteeringBehavior;

            

            if (property.objectReferenceValue == null)
                return;
            Type concreteType = property.objectReferenceValue.GetType();
            UnityEngine.Object wrapped = property.objectReferenceValue;
            wrapped = (UnityEngine.Object)Convert.ChangeType(wrapped, concreteType);

            SerializedObject serializedObject = new SerializedObject(wrapped);

            SerializedProperty isActiveProp = serializedObject.FindProperty("isActive");

            if (behavior.CanToggleActive)
            {
                EditorGUILayout.PropertyField(isActiveProp);
            }

            if (isActiveProp.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                EditorGUILayout.BeginVertical("BOX");

                EditorGUILayout.PropertyField(serializedObject.FindProperty("weight"));

                if (behavior.CanUseTagFilter)
                {
                    DrawTagFilters(serializedObject);
                }

                var editor = Editor.CreateEditor(behavior);
                editor.DrawDefaultInspectorWithoutScriptField();


                SerializedProperty drawDebugProp = serializedObject.FindProperty("drawDebug");
                SerializedProperty drawVectorProp = serializedObject.FindProperty("debugDrawSteering");
                SerializedProperty drawPerceptionProp = serializedObject.FindProperty("debugDrawProperties");

                EditorGUILayout.PropertyField(drawDebugProp);
                if (drawDebugProp.boolValue)
                {
                    EditorGUI.indentLevel++;
                    SerializedProperty color = serializedObject.FindProperty("debugColor");
                    color.colorValue = EditorGUILayout.ColorField(color.colorValue);

                    EditorGUILayout.PropertyField(drawVectorProp);
                    EditorGUILayout.PropertyField(drawPerceptionProp);
                    EditorGUI.indentLevel--;
                    //EditorGUILayout.EndHorizontal();
                }
                                
                GUILayout.Space(5);

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

            }
            serializedObject.ApplyModifiedProperties();
        }


        protected void DrawTagFilters(SerializedObject serializedObject)
        {
            SerializedProperty useTag = serializedObject.FindProperty("useTagFilter");
            EditorGUILayout.PropertyField(useTag);
            if (useTag.boolValue)
            {
                SerializedProperty tags = serializedObject.FindProperty("filterTags");
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.BeginVertical("BOX", GUILayout.ExpandWidth(true));

                for (int i = 0; i < tags.arraySize; i++)
                {
                    GUILayout.BeginHorizontal();

                    tags.GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TagField(tags.GetArrayElementAtIndex(i).stringValue);
                    if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(14)))
                    {
                        tags.DeleteArrayElementAtIndex(i);
                    }
                    GUILayout.EndHorizontal();

                }
                if (GUILayout.Button("+ Add Tag", GUILayout.Width(80)))
                {
                    tags.arraySize = tags.arraySize + 1;
                }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

        }

    }
}
