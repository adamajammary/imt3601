using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour { 

    //private bool     lockCursor = true;
    private float    mouseSensitivity = 10;    
    private float    distanceFromTarget = 4;
    private Vector2  pitchMinMax = new Vector2(-5, 85);
    private float    rotationSmoothTime = 0.1f;

    private Transform   _target;
    Vector3             _rotationSmoothVelocity;
    Vector3             _currentRotation;
    float               _yaw;
    float               _pitch;

	void LateUpdate () {
        if (this._target == null)
            return;

        this._yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        this._pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        this._pitch = Mathf.Clamp(_pitch, pitchMinMax.x, pitchMinMax.y);

        this._currentRotation = Vector3.SmoothDamp(_currentRotation, new Vector3(_pitch, _yaw), 
                                               ref _rotationSmoothVelocity, rotationSmoothTime);
  
        this.transform.eulerAngles = _currentRotation;

        this.transform.position = _target.position - transform.forward * distanceFromTarget;
    }

    public void SetTarget(Transform targetTransform)
    {
        this._target = targetTransform;
    }
}
