using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float _speed = 1;

    void Update()
    {
        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.UpArrow))
            move += Vector3.up;
        if (Input.GetKey(KeyCode.DownArrow))
            move += Vector3.down;
        if (Input.GetKey(KeyCode.LeftArrow))
            move += Vector3.left;
        if (Input.GetKey(KeyCode.RightArrow))
            move += Vector3.right;

        if (move != Vector3.zero)
        {
            transform.position += move * _speed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(move);
        }
    }
}
