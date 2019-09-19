using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ScriptableObject), true)]
public class SteeringBehaviorEditor : Editor
{
    private SteeringBehavior targetBehavior;
    private void OnEnable()
    {
        targetBehavior = (SteeringBehavior)target;

    }
    public override void OnInspectorGUI()
    {
        GUILayout.Label(targetBehavior.name);
    }
}
