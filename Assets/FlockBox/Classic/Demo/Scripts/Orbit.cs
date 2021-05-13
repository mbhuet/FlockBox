using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    public Transform _orbitTarget;
    public float _distance =10;
    public float _speed = 1;
    public float _verticalAngle = 45;

    // Update is called once per frame[aaa
    void Update()
    {
        Vector3 target = _orbitTarget ? _orbitTarget.position : Vector3.zero;
        float x = Mathf.Cos(Time.time * _speed);
        float z = Mathf.Sin(Time.time * _speed);
        transform.position = target + new Vector3(x, 0, z).normalized * _distance;
        transform.LookAt(target);
        transform.RotateAround(target, transform.right, _verticalAngle);
    }
}
