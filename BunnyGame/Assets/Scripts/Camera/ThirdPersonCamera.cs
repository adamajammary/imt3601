using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour { 

    private float       _mouseSensitivity = 10;    
    private float       _distanceFromTarget = 6;
    private float       _curDist;
    private Vector2     _pitchMinMax = new Vector2(-5, 85);
    private float       _rotationSmoothTime = 0.0f;

    private GameObject  _crosshair;
    private Transform   _target;

    Vector3             _rotationSmoothVelocity;
    Vector3             _currentRotation;
    float               _yaw;
    float               _pitch;
  

    void Start()
    {
        _crosshair = GameObject.Find("Crosshair");

        if (_crosshair == null)
            Debug.Log("Could not find _crosshair");

        _crosshair.SetActive(false);
    }


	void LateUpdate () {
        if (this._target == null)
            return;

        this._yaw += Input.GetAxis("Mouse X") * _mouseSensitivity;
        this._pitch -= Input.GetAxis("Mouse Y") * _mouseSensitivity;
        this._pitch = Mathf.Clamp(_pitch, _pitchMinMax.x, _pitchMinMax.y);

        this._currentRotation = Vector3.SmoothDamp(_currentRotation, new Vector3(_pitch, _yaw), 
                                               ref _rotationSmoothVelocity, _rotationSmoothTime);
  
        this.transform.eulerAngles = _currentRotation;

        this.camCollision();
        HandleFpsAim();      
    }

    private void HandleFpsAim()
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            _crosshair.SetActive(true);
            this.transform.position = _target.position;
            _pitchMinMax = new Vector2(-90, 90);
        }
        else 
        {
            _crosshair.SetActive(false);
            this.transform.position = _target.position - transform.forward * this._curDist;
            _pitchMinMax = new Vector2(-5, 85);
        }
    }

    public void SetTarget(Transform targetTransform)
    {
        this._target = targetTransform;
    }

    void camCollision() {
        RaycastHit hit;
        Ray ray = new Ray(this._target.transform.position, this.transform.position - this._target.transform.position);
        int layermask = (1 << 8);
        layermask |= (1 << 11);
        layermask = ~layermask;

        if (Physics.Raycast(ray, out hit, this._distanceFromTarget, layermask)) {
            this._curDist = hit.distance;
        } else
            this._curDist = this._distanceFromTarget;
    }
}
