using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CloudFine
{
    [CustomEditor(typeof(SteeringAgent), true)]
    public class SteeringAgentEditor : Editor
    {
        private bool _dampeningFoldout;

        private SerializedProperty _smoothRotation;
        private SerializedProperty _smoothPosition;

        private SerializedProperty _rotationTension;
        private SerializedProperty _positionTension;

        private SerializedProperty _rotationSlack;
        private SerializedProperty _positionSlack;

        private SerializedProperty _unsmoothGizmo;

        private void Awake()
        {
            _smoothRotation = serializedObject.FindProperty("_smoothRotation");
            _smoothPosition = serializedObject.FindProperty("_smoothPosition");

            _rotationTension = serializedObject.FindProperty("_rotationTension");
            _positionTension = serializedObject.FindProperty("_positionTension");

            _rotationSlack = serializedObject.FindProperty("_rotationSlackDegrees");
            _positionSlack = serializedObject.FindProperty("_positionSlackDistance");

            _unsmoothGizmo = serializedObject.FindProperty("_drawUnsmoothedGizmo");

        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            _dampeningFoldout = EditorGUILayout.Foldout(_dampeningFoldout, "Smoothing", true);
            if (_dampeningFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_smoothRotation);
                if (_smoothRotation.boolValue)
                {
                    _rotationTension.floatValue = EditorGUILayout.Slider("Rotation Tension", _rotationTension.floatValue, 0f, 1f);
                    EditorGUILayout.PropertyField(_rotationSlack, new GUIContent("Rotation Dead Zone"));
                    EditorGUILayout.Space();
                }

                EditorGUILayout.PropertyField(_smoothPosition);

                if (_smoothPosition.boolValue)
                {
                    _positionTension.floatValue = EditorGUILayout.Slider("Position Tension", _positionTension.floatValue, 0f, 1f);
                    EditorGUILayout.PropertyField(_positionSlack, new GUIContent("Position Dead Zone"));
                    EditorGUILayout.Space();
                }
                EditorGUILayout.PropertyField(_unsmoothGizmo);
                EditorGUI.indentLevel--;

            }



            serializedObject.ApplyModifiedProperties();

        }
    }
}
