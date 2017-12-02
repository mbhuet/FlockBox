using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowKeyMovement : MonoBehaviour {
    public float speed;
	// Update is called once per frame
	void Update () {
        Vector2 move = 
            Vector2.up * (Input.GetKey(KeyCode.UpArrow)? 1 : 0) +
            Vector2.left * (Input.GetKey(KeyCode.LeftArrow)? 1 : 0)+
            Vector2.right * (Input.GetKey(KeyCode.RightArrow)? 1 : 0) +
            Vector2.down * (Input.GetKey(KeyCode.DownArrow)? 1 : 0);

        this.transform.Translate(move * Time.deltaTime * speed);
	}
}
