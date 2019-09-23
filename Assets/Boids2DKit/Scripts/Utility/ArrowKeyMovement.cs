using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowKeyMovement : MonoBehaviour {
    public float speed = 10;
	// Update is called once per frame
	void Update () {
        Vector3 move = 
            Vector2.up * (Input.GetKey(KeyCode.UpArrow)? 1 : 0) +
            Vector2.left * (Input.GetKey(KeyCode.LeftArrow)? 1 : 0)+
            Vector2.right * (Input.GetKey(KeyCode.RightArrow)? 1 : 0) +
            Vector2.down * (Input.GetKey(KeyCode.DownArrow)? 1 : 0);

        move = move * Time.deltaTime * speed;
        this.transform.Translate(new Vector3(move.x, move.y, 0));
	}
}
