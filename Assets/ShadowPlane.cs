using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowPlane : MonoBehaviour {

    Transform trackingTransform;

	// Use this for initialization
	void Start () {
        trackingTransform = Camera.main.transform;
        transform.rotation = Quaternion.Euler(-ZLayering.groundTilt, 0, 0);
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = new Vector3(trackingTransform.position.x, trackingTransform.position.y, ZLayering.YtoZPosition(trackingTransform.position.y));
	}
}
