using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class SceneCamControl : MonoBehaviour
{
    public bool move;
    public Transform focusTarget;
    public float focusRotation;
    public float zoomSpeed;

    // Update is called once per frame
    [ExecuteInEditMode]
    void Update()
    {

        if (!move) return;
        Camera cam = Camera.current;
        //SceneView.lastActiveSceneView.pivot = focusTarget.position;
        //SceneView.lastActiveSceneView.rotation = 


        if (!cam) return;
        if (focusTarget)
        {
            cam.transform.RotateAround(focusTarget.position, Vector3.up, focusRotation * Time.unscaledDeltaTime);
        }
        cam.transform.Translate(cam.transform.forward * zoomSpeed * Time.unscaledDeltaTime);


    }
}
