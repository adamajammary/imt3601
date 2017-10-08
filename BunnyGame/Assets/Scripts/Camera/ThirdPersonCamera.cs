using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour { 

    private float       _mouseSensitivity = 10;    
    private float       _distanceFromTarget = 6;
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
        SetFOV(Mathf.Clamp(PlayerPrefs.GetFloat("Field of View", GetComponent<Camera>().fieldOfView), 40, 100));

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
            this.transform.position = _target.position - transform.forward * _distanceFromTarget;
            _pitchMinMax = new Vector2(-5, 85);
        }
    }

    public void SetTarget(Transform targetTransform)
    {
        this._target = targetTransform;
    }

    public void SetFOV(float fov) {
        GetComponent<Camera>().fieldOfView = fov;
    }
}
