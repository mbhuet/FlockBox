using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwitchDampening : MonoBehaviour
{
    [Range(0f, 1f)] public float _rotationTension = .3f;
    [Range(0f, 1f)] public float _positionTension = 1;

    public float _positionSlackDistance = 0;
    public float _rotationSlackDegrees = 10;

    /// <summary>
    /// Will cause the transform to not rotate when still
    /// </summary>
    public bool _positonSlackAffectsRotation;

    Quaternion _lastRotation;
    Vector3 _lastPosition;

    private void Start()
    {
        _lastPosition = transform.position;
        _lastRotation = transform.rotation;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.rotation = _lastRotation;
        transform.position = _lastPosition;

        float rotationSlack = Quaternion.Angle(transform.localRotation, Quaternion.identity)/ _rotationSlackDegrees;
        rotationSlack *= rotationSlack;
        rotationSlack = Mathf.Clamp01(rotationSlack);

        float positionSlack = (transform.localPosition).sqrMagnitude / (_positionSlackDistance * _positionSlackDistance);
        positionSlack *= positionSlack;
        positionSlack = Mathf.Clamp01(positionSlack);

        transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, _positionTension * positionSlack);
        _lastPosition = transform.position;
        
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, _rotationTension * rotationSlack * (_positonSlackAffectsRotation? positionSlack : 1f));
        _lastRotation = transform.rotation;      
    }
}
