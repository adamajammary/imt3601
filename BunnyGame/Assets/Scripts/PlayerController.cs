using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {

    public float walkSpeed = 2;
    public float runSpeed  = 6;
    public float gravity = -12;
    public float jumpHeight = 1;

    [Range(0,1)]
    public float airControlPercent;

    public float turnSmoothTime = 0.2f;
    float turnSmoothVelocity;

    public float speedSmoothTime = 0.2f;
    float speedSmoothVelocity;
    float currentSpeed;
    float velocityY;

    Transform cameraTransform;
    CharacterController controller;

    bool lockCursor = false;

    void Start () {
        this.cameraTransform = Camera.main.transform;
        this.controller = GetComponent<CharacterController>();         
	}
	
	
	void Update () {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 inputDir = input.normalized;
        bool running = Input.GetKey(KeyCode.LeftShift);      

        Move(inputDir, running);
   
        if (Input.GetKey(KeyCode.Space))
            Jump();

        handleMouse();
    }

    void Move(Vector2 inputDir, bool running)
    {
        
        if (inputDir != Vector2.zero) 
        {
            float targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, GetModifiedSmoothTime(turnSmoothTime));
        }
           
        float targetSpeed = ((running) ? runSpeed : walkSpeed) * inputDir.magnitude;
        this.currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, GetModifiedSmoothTime(speedSmoothTime));

        this.velocityY += Time.deltaTime * gravity;
      
        Vector3 velocity = transform.forward * currentSpeed + Vector3.up * velocityY;

        this.controller.Move(velocity * Time.deltaTime);

        if (controller.isGrounded)
            velocityY = 0;
    }


    void Jump()
    {
        if(controller.isGrounded)
        {
            float jumpVelocity = Mathf.Sqrt(-2 * gravity * jumpHeight); // Kinnematik equation
            this.velocityY = jumpVelocity;
        }
    }

    //Controll player in air after jump
    float GetModifiedSmoothTime(float smoothTime)
    {
        if (controller.isGrounded)
            return smoothTime;

        if (smoothTime == 0)
            return float.MaxValue;

        return smoothTime / airControlPercent;        
    }

    void handleMouse()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            lockCursor = !lockCursor;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
