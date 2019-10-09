using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;

namespace CloudFine
{
    [CustomEditor(typeof(FlockBox), true)]
    public class FlockBoxEditor : Editor
    {
        
        private SerializedProperty _dimensionX;
        private SerializedProperty _dimensionY;
        private SerializedProperty _dimensionZ;

        private SerializedProperty _size;
        private SerializedProperty _buffer;
        private SerializedProperty _wrap;
        private SerializedProperty _sleep;

        private SerializedProperty _populations;
        private SerializedProperty _gizmos;



        private bool optimizationFoldout = false;
        private bool debugFoldout = false;


        private void OnEnable()
        {
            _dimensionX = serializedObject.FindProperty("dimensions_x");
            _dimensionY = serializedObject.FindProperty("dimensions_y");
            _dimensionZ = serializedObject.FindProperty("dimensions_z");

            _size = serializedObject.FindProperty("cellSize");
            _buffer = serializedObject.FindProperty("boundaryBuffer");
            _wrap = serializedObject.FindProperty("wrapEdges");
            _sleep = serializedObject.FindProperty("sleepChance");
            _populations = serializedObject.FindProperty("startingPopulations");
            _gizmos = serializedObject.FindProperty("drawGizmos");

        }

        public override void OnInspectorGUI()
        {
            Vector3Int dimensions = EditorGUILayout.Vector3IntField("Dimensions", new Vector3Int(_dimensionX.intValue, _dimensionY.intValue, _dimensionZ.intValue));
            dimensions.x = Math.Max(dimensions.x, 0);
            dimensions.y = Math.Max(dimensions.y, 0);
            dimensions.z = Math.Max(dimensions.z, 0);
            _dimensionX.intValue = dimensions.x;
            _dimensionY.intValue = dimensions.y;
            _dimensionZ.intValue = dimensions.z;

            EditorGUILayout.PropertyField(_size);
            EditorGUILayout.PropertyField(_wrap);
           
            if (!_wrap.boolValue)
            {
                EditorGUILayout.PropertyField(_buffer);
                if (dimensions.x > 0 || dimensions.y > 0 || dimensions.z > 0)
                {
                    _buffer.floatValue = Mathf.Clamp(_buffer.floatValue,
                        0,
                        Mathf.Min(
                            dimensions.x > 0 ? dimensions.x : float.MaxValue,
                            (dimensions.y > 0 ? dimensions.y : float.MaxValue),
                            (dimensions.z > 0 ? dimensions.z : float.MaxValue)
                            )
                            * _size.floatValue / 2f
                    );
                }
                
            }
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_populations, true);

            EditorGUILayout.Space();
            optimizationFoldout = EditorGUILayout.Foldout(optimizationFoldout, "Optimization", true);
            if (optimizationFoldout)
            {
                EditorGUILayout.Slider(_sleep, 0, 1);

            }
            EditorGUILayout.Space();

            debugFoldout = EditorGUILayout.Foldout(debugFoldout, "Debug", true);
            if (debugFoldout)
            {
                EditorGUILayout.PropertyField(_gizmos);
            }


            serializedObject.ApplyModifiedProperties();
        }

        
    }
}