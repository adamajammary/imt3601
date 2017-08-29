/*
using System.Collections;
using System.Collections.Generic;
*/
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public Transform cameraTransform;

    Vector3 camPos, forward;
    Rigidbody rb;
    float speed;
    float distance, angle1, angle2;
    bool lockCursor = false;
    // Use this for initialization
    void Start() {
        speed = 10.0f;
        angle1 = 0.0f;
        angle2 = 0.0f;
        distance = 5.0f;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {
        setCameraPos();
        movePlayer();
        handleMouse();
    }

    //Takes keyboard input to move player
    void movePlayer() {
        Vector3 force = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) {
            float angle = angle1 - Mathf.PI;
            forward.x = Mathf.Cos(angle);
            forward.z = Mathf.Sin(angle);
            forward.y = 0;
            force += forward * speed;
            transform.eulerAngles = new Vector3(0, -Mathf.Rad2Deg*angle1 + 90, 0);
        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            force.y += speed*30;
        }
        rb.AddForce(force);
    }

    //Sets the position of the camera, the camera is positioned on a sphere centered on the target, 
    //looking at the target transform. Mouse delta is used to change the position
    void setCameraPos() {
        angle1 -= Input.GetAxis("Mouse X") * Time.deltaTime * 2.0f;
        angle2 += Input.GetAxis("Mouse Y") * Time.deltaTime * 2.0f;
        distance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 100.0f;

        if (angle2 > Mathf.PI - 0.5f) angle2 = Mathf.PI - 0.5f;
        if (angle2 < 0.1f) angle2 = 0.1f;

        camPos.x = transform.position.x + distance * Mathf.Cos(angle1) * Mathf.Sin(angle2);
        camPos.z = transform.position.z + distance * Mathf.Sin(angle1) * Mathf.Sin(angle2);
        camPos.y = transform.position.y + distance * Mathf.Cos(angle2);

        cameraTransform.position = camPos;
        cameraTransform.LookAt(transform);
    }

    //Handles mouse, pressing esc toggles cursor visibility
    void handleMouse() {
        if (Input.GetKeyDown(KeyCode.Escape))
            lockCursor = !lockCursor;

        if (lockCursor) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        } else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}