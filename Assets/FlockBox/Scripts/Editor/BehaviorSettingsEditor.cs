using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;

namespace CloudFine
{
    [CustomEditor(typeof(BehaviorSettings), true)]
    public class BehaviorSettingsEditor : Editor
    {
        const string activeStyle = "ProgressBarBar";
        const string inactiveStyle = "ProgressBarBack";

        private BehaviorSettings targetSettings;
        private SerializedProperty _behaviors;
        private SerializedProperty _maxForce;
        private SerializedProperty _maxSpeed;
        private SerializedProperty _containment;

        private int toRemove = -1;

        private void OnEnable()
        {
            targetSettings = (BehaviorSettings)target;
            _behaviors = serializedObject.FindProperty("behaviors");
            _maxForce = serializedObject.FindProperty("maxForce");
            _maxSpeed = serializedObject.FindProperty("maxSpeed");
            _containment = serializedObject.FindProperty("containmentBehavior");
        }

        public override void OnInspectorGUI()
        {
            if (_containment.objectReferenceValue == null)
            {
                if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(target)))
                {
                    _containment.objectReferenceValue = CreateBehavior(typeof(ContainmentBehavior));
                }
            }

            EditorGUILayout.PropertyField(_maxSpeed);
            EditorGUILayout.PropertyField(_maxForce);


            DrawBehaviorBox(targetSettings.Containment, _containment, -1, false);

            for (int i = 0; i < _behaviors.arraySize; i++)
            {
                DrawBehaviorBox(targetSettings.GetBehavior(i), _behaviors.GetArrayElementAtIndex(i), i, true);
            }

            if (toRemove >= 0)
            {
                AssetDatabase.RemoveObjectFromAsset(_behaviors.GetArrayElementAtIndex(toRemove).objectReferenceValue);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();

                _behaviors.DeleteArrayElementAtIndex(toRemove);
                _behaviors.DeleteArrayElementAtIndex(toRemove);
                toRemove = -1;
            }


            GUILayout.BeginVertical("BOX");
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Behavior", GUILayout.Width(130)))
            {
                GenericMenu menu = new GenericMenu();
                List<SteeringBehavior> behaviors = targetSettings.Behaviors.ToList();
                foreach (Type type in System.AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(SteeringBehavior)))
                {
                    if (type.IsAbstract) continue;
                    if (behaviors.Any(x => x.GetType() == type) || type == typeof(ContainmentBehavior))
                    {
                        menu.AddDisabledItem(new GUIContent(type.Name), false);
                    }
                    else
                    {
                        menu.AddItem(new GUIContent(type.Name), false, AddBehavior, type);
                    }
                }
                menu.ShowAsContext();

            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawBehaviorBox(SteeringBehavior behavior, SerializedProperty property, int i, bool canRemove)
        {
            if (!behavior) return;
            EditorGUILayout.BeginVertical("BOX");
            EditorGUILayout.BeginHorizontal(behavior.IsActive ? activeStyle : inactiveStyle);
            GUILayout.Space(20);

            SerializedObject behaviorObject = new SerializedObject(property.objectReferenceValue);
            SerializedProperty foldoutProperty = behaviorObject.FindProperty("foldout");

            bool foldout = foldoutProperty.boolValue;

            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, behavior.GetType().Name);
            EditorGUILayout.EndFoldoutHeaderGroup();
            foldoutProperty.boolValue = foldout;

            behaviorObject.ApplyModifiedProperties();

            GUILayout.FlexibleSpace();
            if (canRemove)
            {
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    toRemove = i;
                }
            }
            
            EditorGUILayout.EndHorizontal();

            if (foldout)
            {
                GUILayout.Space(-20);

                EditorGUILayout.PropertyField(property);
            }


            GUILayout.Space(5);

            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }


        SteeringBehavior CreateBehavior(object behaviorType)
        {
            SteeringBehavior newBehavior = (SteeringBehavior)ScriptableObject.CreateInstance((Type)behaviorType);
            newBehavior.hideFlags = HideFlags.HideInHierarchy;

            AssetDatabase.AddObjectToAsset(newBehavior, AssetDatabase.GetAssetPath(target));
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newBehavior));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return newBehavior;
        }

        void AddBehavior(object behaviorType)
        {

                _behaviors.arraySize = _behaviors.arraySize + 1;
                _behaviors.GetArrayElementAtIndex(_behaviors.arraySize - 1).objectReferenceValue = (UnityEngine.Object)CreateBehavior(behaviorType);
                serializedObject.ApplyModifiedProperties();

        }
    }
}