using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {

    public float walkSpeed = 5;
    public float runSpeed = 12;
    public float gravity = -12;
    public float jumpHeight = 3;

    [Range(0, 1)]
    public float airControlPercent = 0.5f;

    public float turnSmoothTime = 0.2f;
    public float speedSmoothTime = 0.2f;

    private float _turnSmoothVelocity;
    private float _speedSmoothVelocity;
    private float _currentSpeed;
    private float _velocityY;

    private Transform _cameraTransform;
    private CharacterController _controller;

    bool lockCursor = false;

    public void onWaterStay(float waterForce) {
        this._velocityY += waterForce * Time.deltaTime;
    }

    private void Start() {
        this._cameraTransform = Camera.main.transform;
        this._controller = this.GetComponent<CharacterController>();

        this._bunnyCommands = this.GetComponent<BunnyCommands>();
        this.airControlPercent = 1;


        this.spawn();
    }


    private void Update() {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 inputDir = input.normalized;
        bool running = Input.GetKey(KeyCode.LeftShift);

        Move(inputDir, running);

        if (Input.GetAxisRaw("Jump") > 0)
            this.jump();

        
        HandleAiming();

        handleMouse();
    }

    // Turn off and on MeshRenderer so FPS camera works
    private void HandleAiming(){
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            foreach (Transform t in this.gameObject.transform.GetChild(0))
            {
                t.gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
        }
        else if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            foreach (Transform t in this.gameObject.transform.GetChild(0))
            {
                t.gameObject.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }



    void Move(Vector2 inputDir, bool running) {

        if (inputDir != Vector2.zero) {
            float targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation,
                                                ref _turnSmoothVelocity, GetModifiedSmoothTime(turnSmoothTime));
        }

        float targetSpeed = ((running) ? runSpeed : walkSpeed) * inputDir.magnitude;
        this._currentSpeed = Mathf.SmoothDamp(_currentSpeed, targetSpeed, ref _speedSmoothVelocity, GetModifiedSmoothTime(speedSmoothTime));

        this._velocityY += Time.deltaTime * gravity;

        Vector3 velocity = transform.forward * _currentSpeed + Vector3.up * _velocityY;

        this._controller.Move(velocity * Time.deltaTime);

        if (_controller.isGrounded)
            _velocityY = 0;
    }

    

    private void jump() {
        if (_controller.isGrounded) {
            float jumpVelocity = Mathf.Sqrt(-2 * gravity * jumpHeight); 
            this._velocityY = jumpVelocity;
        }
    }

    //Controll player in air after jump
    private float GetModifiedSmoothTime(float smoothTime) {
        if (_controller.isGrounded)
            return smoothTime;

        if (smoothTime == 0)
            return float.MaxValue;

        return smoothTime / airControlPercent;
    }

    private void handleMouse() {
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

    private void spawn() {
        transform.position = new Vector3(Random.Range(-40, 40),
                                         10,
                                         Random.Range(-40, 40));
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.tag == "projectile") {
            this.spawn();
            Destroy(other.gameObject);
        }

    }
}
