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

        private const float RemoveTagButtonWidth = 20;
        private const float AddTagButtonWidth = 80;


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

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!init) Initialize(property);

            float height = 0;

            if (behavior.CanToggleActive)
            {
                height += EditorGUI.GetPropertyHeight(isActiveProp);
            }


            if (isActiveProp.boolValue)
            {
                height += EditorGUIUtility.singleLineHeight; ;

                height += EditorGUI.GetPropertyHeight(weight);
                if (behavior.CanUseTagFilter)
                {
                    height += EditorGUI.GetPropertyHeight(useTag);
                    if (useTag.boolValue)
                    {
                        height += EditorGUIUtility.singleLineHeight;

                        for (int i = 0; i < tags.arraySize; i++)
                        {
                            height += EditorGUIUtility.singleLineHeight;
                        }
                        height += EditorGUIUtility.singleLineHeight; //add button
                    }

                }

                Iterator.Reset();
                Iterator.NextVisible(true);

                while (Iterator.NextVisible(false))
                {
                    height+=EditorGUI.GetPropertyHeight(Iterator);
                }
                height += EditorGUI.GetPropertyHeight(drawDebugProp);

                if (drawDebugProp.boolValue)
                {
                    height += EditorGUI.GetPropertyHeight(color);
                    height += EditorGUI.GetPropertyHeight(drawVectorProp);
                    height += EditorGUI.GetPropertyHeight(drawPerceptionProp);

                }
            }
            height += 5;
            return height;

        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!init) Initialize(property);

            if (property.objectReferenceValue == null)
                return;

            float heightOffset = position.y;
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.indentLevel++;


            if (behavior.CanToggleActive)
            {
                DoProp(isActiveProp, ref position);
            }



            if (isActiveProp.boolValue)
            {
                position.y += EditorGUIUtility.singleLineHeight; ;
                EditorGUI.indentLevel++;

                DoProp(weight, ref position);

               
                if (behavior.CanUseTagFilter)
                {
                    DrawTagFilters(ref position);
                }

                DrawDefaultInspectorWithoutScriptField(ref position);

                DoProp(drawDebugProp, ref position);
           

                if (drawDebugProp.boolValue)
                {
                    EditorGUI.indentLevel++;
                    DoProp(color, ref position);
                    DoProp(drawVectorProp, ref position);
                    DoProp(drawPerceptionProp, ref position);
                    EditorGUI.indentLevel--;
                }


                EditorGUI.indentLevel--;

            }
            serializedObject.ApplyModifiedProperties();
            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();

        }

        private void DoProp(SerializedProperty prop, ref Rect position)
        {
            Rect rect = new Rect(position);
            float height = EditorGUI.GetPropertyHeight(prop);
            rect.height = height;
            EditorGUI.PropertyField(rect, prop);
            position.y += height;
        }


        protected void DrawTagFilters(ref Rect position)
        {
            DoProp(useTag, ref position);

            if (useTag.boolValue)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < tags.arraySize; i++)
                {
                    Rect tagRect = new Rect(position);
                    float height = EditorGUIUtility.singleLineHeight;
                    tagRect.height = height;
                    tagRect.width = position.width - RemoveTagButtonWidth;

                    tags.GetArrayElementAtIndex(i).stringValue = EditorGUI.TagField(tagRect, tags.GetArrayElementAtIndex(i).stringValue);

                    Rect removeRect = new Rect(position);
                    removeRect.x = position.width;
                    removeRect.height = height;
                    removeRect.width = RemoveTagButtonWidth;
                    position.y += height;

                    if (GUI.Button(removeRect, "X"))
                    {
                        tags.DeleteArrayElementAtIndex(i);
                    }
                }


                Rect addRect = new Rect(position);
                addRect.width = AddTagButtonWidth;
                addRect.x = position.width - AddTagButtonWidth;
                addRect.height = EditorGUIUtility.singleLineHeight;

                if (GUI.Button(addRect, "+ Add Tag"))
                {
                    tags.arraySize = tags.arraySize + 1;
                }
                position.y += addRect.height;
                position.y += EditorGUIUtility.singleLineHeight;

                EditorGUI.indentLevel--;
            }

        }

        private void DrawDefaultInspectorWithoutScriptField(ref Rect position)
        {
            Iterator.Reset();
            Iterator.NextVisible(true);

            while (Iterator.NextVisible(false))
            {
                DoProp(Iterator, ref position);
            }
        }
    }
}
