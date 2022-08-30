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

        private Vector3[] faces;

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

            faces = new Vector3[]
{
                Vector3.left,
                Vector3.right,
                Vector3.up,
                Vector3.down,
                Vector3.forward,
                Vector3.back
};
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

            Vector3 dimensions = new Vector3(_dimensionX.floatValue, _dimensionY.floatValue, _dimensionZ.floatValue) * _size.floatValue;

            dimensions.x = Mathf.Round(dimensions.x * 1000) / 1000f;
            dimensions.y = Mathf.Round(dimensions.y * 1000) / 1000f;
            dimensions.z = Mathf.Round(dimensions.z * 1000) / 1000f;

            dimensions = EditorGUILayout.Vector3Field("Dimensions", dimensions);
            float size = EditorGUILayout.FloatField("Cell Size", _size.floatValue);

            float maxDimension = Mathf.Max(dimensions.x, dimensions.y, dimensions.z);
            size = Mathf.Max(maxDimension * .01f, size);

            dimensions /= size;

            dimensions.x = Math.Max(dimensions.x, 0);
            dimensions.y = Math.Max(dimensions.y, 0);
            dimensions.z = Math.Max(dimensions.z, 0);

            _dimensionX.floatValue = dimensions.x;
            _dimensionY.floatValue = dimensions.y;
            _dimensionZ.floatValue = dimensions.z;
            _size.floatValue = size;

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
                EditorGUILayout.PropertyField(_populations);
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


        public void OnSceneGUI()
        {
            Transform transform = (target as FlockBox).transform;
            Vector3 dimensions = new Vector3(_dimensionX.floatValue, _dimensionY.floatValue, _dimensionZ.floatValue);
            float size = _size.floatValue;

            Handles.matrix = transform.localToWorldMatrix;
            Handles.color = Color.grey * .5f;

            Camera sceneCam = SceneView.lastActiveSceneView.camera;
            Vector3 camForward = Handles.matrix.inverse.MultiplyVector(sceneCam.transform.forward);
            Vector3 camPos = Handles.matrix.inverse.MultiplyPoint(sceneCam.transform.position);
            bool isOrtho = sceneCam.orthographic;

            Vector3 dimensionsDelta = Vector3.zero;
            Vector3 posDelta = Vector3.zero;

            for (int f = 0; f<faces.Length; f++)
            {
                Vector3 faceNormal = faces[f];
                Vector3 faceCenter = Vector3.Scale((Vector3.one + faceNormal) * .5f, (Vector3)dimensions * size);
                Vector3 faceToCam = camPos - faceCenter;

                bool facingCamera = isOrtho? Vector3.Dot(camForward, faceNormal) <= .000001f : Vector3.Dot(faceNormal, faceToCam) > 0;

                if (facingCamera)
                {
                    Handles.color = Color.grey * .75f;

                    Vector3 dir1 = Vector3.zero;
                    Vector3 dir2 = Vector3.zero;

                    if (faceNormal.x != 0)
                    {
                        float x = faceNormal.x < 0 ? 0 : dimensions.x;
                        for(int y = 0; y < dimensions.y; y++)
                        {
                            Handles.DrawAAPolyLine(
                                new Vector3(x, y, 0) * size,
                                new Vector3(x, y, dimensions.z) * size
                                );                            
                        }
                        for (int z = 0; z < dimensions.z; z++)
                        {
                            Handles.DrawAAPolyLine(
                                new Vector3(x, 0, z) * size,
                                new Vector3(x, dimensions.y, z) * size
                                );
                        }
                        dir1 = Vector3.up;
                        dir2 = Vector3.forward;
                    }

                    if (faceNormal.y != 0)
                    {
                        float y = faceNormal.y < 0 ? 0 : dimensions.y;
                        for (int x = 0; x < dimensions.x; x++)
                        {
                            Handles.DrawAAPolyLine(
                                new Vector3(x, y, 0) * size,
                                new Vector3(x, y, dimensions.z) * size
                                );                  
                        }
                        for (int z = 0; z < dimensions.z; z++)
                        {
                            Handles.DrawAAPolyLine(
                                new Vector3(0, y, z) * size,
                                new Vector3(dimensions.x, y, z) * size
                                );
                        }
                        dir1 = Vector3.right;
                        dir2 = Vector3.forward;
                    } 
                    
                    if(faceNormal.z != 0)
                    {
                        float z = faceNormal.z < 0 ? 0 : dimensions.z;                      
                        for (int x = 0; x < dimensions.x; x++)
                        {
                            Handles.DrawAAPolyLine(
                                new Vector3(x, 0, z) * size,
                                new Vector3(x, dimensions.y, z) * size
                                );                       
                        }                      
                        for (int y = 0; y < dimensions.y; y++)
                        {
                            Handles.DrawAAPolyLine(
                                new Vector3(0, y, z) * size,
                                new Vector3(dimensions.x, y, z) * size
                                );
                        }
                        dir1 = Vector3.right;
                        dir2 = Vector3.up;
                    }                                
                }

                if (facingCamera)
                {
                    Handles.color = Color.white;
                }
                else
                {
                    Handles.color = Color.grey;
                }

                Vector3 worldDimesions = (Vector3)dimensions * size;

                Vector3 handlePos = faceCenter;// + Vector3.Scale(worldDimesions,dir1) * .5f + Vector3.Scale(worldDimesions,dir2) * .5f;
                float handleSize = HandleUtility.GetHandleSize(handlePos) * .05f;

                Vector3 drag = Handles.Slider(faceCenter, faceNormal, handleSize, Handles.DotHandleCap, 0);

                Vector3 delta = (drag - handlePos);
                Vector3 localSpaceDelta = Handles.matrix.MultiplyVector(delta);

                if(faceNormal.x < 0 || faceNormal.y <0 || faceNormal.z < 0)
                {
                    posDelta += localSpaceDelta;
                    delta = -delta;
                }
                dimensionsDelta += delta/size;
            }

            dimensions += dimensionsDelta;

            dimensions.x = Math.Max(dimensions.x, 0);
            dimensions.y = Math.Max(dimensions.y, 0);
            dimensions.z = Math.Max(dimensions.z, 0);

            _dimensionX.floatValue = dimensions.x;
            _dimensionY.floatValue = dimensions.y;
            _dimensionZ.floatValue = dimensions.z;

            Undo.RecordObject(transform, "Move Transform");
            transform.position += posDelta;

            serializedObject.ApplyModifiedProperties();
        }
    }
}