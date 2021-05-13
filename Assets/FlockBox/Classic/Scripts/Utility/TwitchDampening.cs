using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwitchDampening : MonoBehaviour
{
    [Range(0f, 1f)] public float _rotationTension = .95f;
    [Range(0f, 1f)] public float _positionTension = 1;

    public float _positionSlackDistance = 0;
    public float _rotationSlackDegrees = 5;

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

        transform.localPosition = SmoothedPosition(_initialLocalPosition);
        transform.localRotation = SmoothedRotation(_initialLocalRotation);

        _lastWorldRotation = transform.rotation;
        _lastWorldPosition = transform.position;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="desiredLocalPosition"></param>
    /// <returns></returns>
    protected Vector3 SmoothedPosition(Vector3 desiredLocalPosition)
    {
        float positionSlack = 1f;
        if (_positionSlackDistance > 0)
        {
            positionSlack = (transform.localPosition - desiredLocalPosition).sqrMagnitude / (_positionSlackDistance * _positionSlackDistance);
        }
        positionSlack *= positionSlack;
        positionSlack = Mathf.Clamp01(positionSlack);

        return Vector3.Lerp(transform.localPosition, desiredLocalPosition, 1f - Mathf.Pow((1f - _positionTension * positionSlack), Time.deltaTime));
    }

    protected Quaternion SmoothedRotation(Quaternion desiredLocalRotation)
    {
        float rotationSlack = 1;
        if (_rotationSlackDegrees > 0)
        {
            rotationSlack = Quaternion.Angle(transform.localRotation, desiredLocalRotation) / _rotationSlackDegrees;
        }
        rotationSlack *= rotationSlack;
        rotationSlack = Mathf.Clamp01(rotationSlack);

        return Quaternion.Slerp(transform.localRotation, desiredLocalRotation, 1f - Mathf.Pow(1f - (_rotationTension * rotationSlack), Time.deltaTime));
    }
}
