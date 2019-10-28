using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SceneCamControl))]
public class SceneCamControlEditor : Editor
{
    private System.DateTime last = System.DateTime.Now;
    private double rot;
    private void OnSceneGUI()
    {
        //SceneView.lastActiveSceneView.LookAt(Vector3.zero, Quaternion.Euler(45, System.DateTime.Now.Second * 10 % 360, 0));
        var delta = (System.DateTime.Now - last).TotalSeconds;
        last = System.DateTime.Now;
        rot += delta;
        SceneView.lastActiveSceneView.pivot = Vector3.zero;
        SceneView.lastActiveSceneView.rotation = Quaternion.Euler(45, (float)rot *10, 0);
        SceneView.RepaintAll();

    }
}
