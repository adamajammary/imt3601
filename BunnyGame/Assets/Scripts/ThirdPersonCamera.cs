using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour {
    

    public bool lockCursor;
    public float mouseSensitivity = 10;
    private Transform target;
    public float distanceFromTarget = 2;
    public Vector2 pitchMinMax = new Vector2(-5, 85);
    public float rotationSmoothTime = 0.1f;
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;

    float yaw;
    float pitch;

	void LateUpdate () {

        this.yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        this.pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        this.pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        this.currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
  
        this.transform.eulerAngles = currentRotation;

        this.transform.position = target.position - transform.forward * distanceFromTarget;
    }

    public void SetTarget(Transform targetTransform)
    {
        this.target = targetTransform;
    }
}
