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

    private Quaternion _lastWorldRotation;
    private Vector3 _lastWorldPosition;

    private Quaternion _initialLocalRotation;
    private Vector3 _initialLocalPosition;

    private Vector3 _positionDampVelocity;
    private Quaternion _rotationDampVelocity;

    private float _positionDampTime;
    private float _rotaitonDampTime;

    private void Start()
    {
        _initialLocalRotation = transform.localRotation;
        _initialLocalPosition = transform.localPosition;

        _lastWorldPosition = transform.position;
        _lastWorldRotation = transform.rotation;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.rotation = _lastWorldRotation;
        transform.position = _lastWorldPosition;

        float rotationSlack = Quaternion.Angle(transform.localRotation, _initialLocalRotation)/ _rotationSlackDegrees;
        rotationSlack *= rotationSlack;
        rotationSlack = Mathf.Clamp01(rotationSlack);

        float positionSlack = (transform.localPosition - _initialLocalPosition).sqrMagnitude / (_positionSlackDistance * _positionSlackDistance);
        positionSlack *= positionSlack;
        positionSlack = Mathf.Clamp01(positionSlack);

        //lerp(target, value, expE(-rate[0-1] * deltaTime))
        transform.localPosition = Vector3.Lerp(transform.localPosition, _initialLocalPosition, Mathf.Exp( -(_positionTension * positionSlack) * Time.deltaTime));
        transform.localRotation = Quaternion.Slerp(transform.localRotation, _initialLocalRotation, Mathf.Exp(-(_rotationTension * rotationSlack * (_positonSlackAffectsRotation? positionSlack : 1f)) * Time.deltaTime));

        _lastWorldRotation = transform.rotation;
        _lastWorldPosition = transform.position;

    }
}
