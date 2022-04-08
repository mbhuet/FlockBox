using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using UnityEditorInternal;

namespace CloudFine.FlockBox
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
        private SerializedProperty _worldSpace;

        private SerializedProperty _populations;
        private SerializedProperty _drawBoundary;
        private SerializedProperty _drawOccupiedCells;

        private SerializedProperty _cellCapacity;
        private SerializedProperty _useCellCapacity;
        private SerializedProperty _useDOTS;

        private bool optimizationFoldout = false;
        private bool debugFoldout = false;

        ReorderableList populationList;

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
            _drawBoundary = serializedObject.FindProperty("drawBoundary");
            _drawOccupiedCells = serializedObject.FindProperty("drawOccupiedCells");

            _cellCapacity = serializedObject.FindProperty("maxCellCapacity");
            _useCellCapacity = serializedObject.FindProperty("capCellCapacity");
            _useDOTS = serializedObject.FindProperty("useDOTS");
            _worldSpace = serializedObject.FindProperty("useWorldSpace");

            populationList = new ReorderableList(serializedObject, _populations, true, true, true, true);

            populationList.drawElementCallback = DrawPopulationListItems; // Delegate to draw the elements on the list
            populationList.drawHeaderCallback = DrawPopulationListHeader; // Skip this line if you set displayHeader to 'false' in your ReorderableList constructor.
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            SerializedProperty prop = serializedObject.FindProperty("m_Script");
            EditorGUILayout.PropertyField(prop, true, new GUILayoutOption[0]);
            GUI.enabled = true;

#if FLOCKBOX_DOTS
            EditorGUILayout.BeginHorizontal();
            BehaviorSettingsEditor.DOTSBadge();

            if (Application.isPlaying)
            {
                GUI.enabled = false;
            }
            EditorGUILayout.PropertyField(_useDOTS);

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            
            if (_useDOTS.boolValue)
            {
                EditorGUILayout.HelpBox(new GUIContent("Note: Some features may not be available in DOTS mode. See manual for more information."));
            }
#endif

            Vector3Int dimensions = EditorGUILayout.Vector3IntField("Dimensions", new Vector3Int(_dimensionX.intValue, _dimensionY.intValue, _dimensionZ.intValue));
            dimensions.x = Math.Max(dimensions.x, 0);
            dimensions.y = Math.Max(dimensions.y, 0);
            dimensions.z = Math.Max(dimensions.z, 0);
            _dimensionX.intValue = dimensions.x;
            _dimensionY.intValue = dimensions.y;
            _dimensionZ.intValue = dimensions.z;

            EditorGUILayout.PropertyField(_size);
            EditorGUILayout.PropertyField(_worldSpace, new GUIContent("World Space Flocking"));
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

            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                populationList.DoLayoutList();
            }

            optimizationFoldout = EditorGUILayout.Foldout(optimizationFoldout, "Optimization", true);
            if (optimizationFoldout)
            {
                EditorGUI.indentLevel = 1;

                EditorGUILayout.Slider(_sleep, 0, 1);
                EditorGUILayout.PropertyField(_useCellCapacity);
                if (_useCellCapacity.boolValue)
                {
                    EditorGUI.indentLevel = 2;
                    EditorGUILayout.PropertyField(_cellCapacity);
                }

                EditorGUI.indentLevel = 0;
            }
            EditorGUILayout.Space();

            debugFoldout = EditorGUILayout.Foldout(debugFoldout, "Debug", true);
            if (debugFoldout)
            {
                EditorGUI.indentLevel = 1;
                EditorGUILayout.PropertyField(_drawBoundary);
                EditorGUILayout.PropertyField(_drawOccupiedCells);

                EditorGUI.indentLevel = 0;

            }


            serializedObject.ApplyModifiedProperties();
        }


        void DrawPopulationListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = populationList.serializedProperty.GetArrayElementAtIndex(index); //The element in the list


            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width*(2f/3f), EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("prefab"),
                GUIContent.none
            );

            // The 'level' property
            // The label field for level (width 100, height of a single line)
            //EditorGUI.LabelField(new Rect(rect.x + 120, rect.y, 100, EditorGUIUtility.singleLineHeight), "Population");

            //The property field for level. Since we do not need so much space in an int, width is set to 20, height of a single line.

            EditorGUI.PropertyField(
                new Rect(rect.x + rect.width*(2f/3f), rect.y, rect.width*(1f/3f), EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("population"),
                GUIContent.none
            );
        }

        void DrawPopulationListHeader(Rect rect)
        {
            string name = "Starting Populations";
            EditorGUI.LabelField(rect, name);
        }


    }
}