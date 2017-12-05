using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugToggles : MonoBehaviour {

	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.T)) RelationshipBehavior.drawConnections = !RelationshipBehavior.drawConnections;
        if (Input.GetKey(KeyCode.Escape)) Application.Quit();
	}
}
