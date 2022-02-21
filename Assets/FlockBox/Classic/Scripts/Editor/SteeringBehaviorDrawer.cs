using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace CloudFine.FlockBox
{
    [CustomPropertyDrawer(typeof(SteeringBehavior), true)]
    public class SteeringBehaviorDrawer : PropertyDrawer
    {
        private bool init;
        SteeringBehavior behavior;
        Type concreteType;
        UnityEngine.Object wrapped;
        SerializedObject serializedObject;
        SerializedProperty isActiveProp;

        SerializedProperty drawDebugProp;
        SerializedProperty drawVectorProp;
        SerializedProperty drawPerceptionProp;
        SerializedProperty weight;
        SerializedProperty useTag;
        SerializedProperty tags;

        SerializedProperty color;
        SerializedProperty Iterator;


        private void Initialize(SerializedProperty property)
        {
            if (property.objectReferenceValue == null)
                return;

            behavior = property.objectReferenceValue as SteeringBehavior;
            concreteType = property.objectReferenceValue.GetType();
            wrapped = property.objectReferenceValue;
            wrapped = (UnityEngine.Object)Convert.ChangeType(wrapped, concreteType);

            serializedObject = new SerializedObject(wrapped);
            isActiveProp = serializedObject.FindProperty("isActive");

            drawDebugProp = serializedObject.FindProperty("drawDebug");
            drawVectorProp = serializedObject.FindProperty("debugDrawSteering");
            drawPerceptionProp = serializedObject.FindProperty("debugDrawProperties");
            weight = serializedObject.FindProperty("weight");
            useTag = serializedObject.FindProperty("useTagFilter");
            tags = serializedObject.FindProperty("filterTags");

            color = serializedObject.FindProperty("debugColor");
            Iterator = serializedObject.GetIterator();


            init = true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!init) Initialize(property);

            if (property.objectReferenceValue == null)
                return;

            
            if (behavior.CanToggleActive)
            {
                EditorGUILayout.PropertyField(isActiveProp);
            }

            if (isActiveProp.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                EditorGUILayout.BeginVertical("BOX");

                EditorGUILayout.PropertyField(weight);

                if (behavior.CanUseTagFilter)
                {
                    DrawTagFilters();
                }

                DrawDefaultInspectorWithoutScriptField();
               

                EditorGUILayout.PropertyField(drawDebugProp);
                if (drawDebugProp.boolValue)
                {
                    EditorGUI.indentLevel++;
                    color.colorValue = EditorGUILayout.ColorField(color.colorValue);

                    EditorGUILayout.PropertyField(drawVectorProp);
                    EditorGUILayout.PropertyField(drawPerceptionProp);
                    EditorGUI.indentLevel--;
                }
                                
                GUILayout.Space(5);

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

            }
            serializedObject.ApplyModifiedProperties();
        }


        protected void DrawTagFilters()
        {
            EditorGUILayout.PropertyField(useTag);
            if (useTag.boolValue)
            {
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

        private void DrawDefaultInspectorWithoutScriptField()
        {
            Iterator.Reset();
            Iterator.NextVisible(true);

            while (Iterator.NextVisible(false))
            {
                EditorGUILayout.PropertyField(Iterator, true);
            }
        }

    }
}
