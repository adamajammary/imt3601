using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {

    public float walkSpeed = 4;
    public float runSpeed = 8;
    public float gravity = -12;
    public float jumpHeight = 1;

    [Range(0, 1)]
    public float airControlPercent;

    public float turnSmoothTime = 0.2f;
    public float speedSmoothTime = 0.2f;

    private float _turnSmoothVelocity;
    private float _speedSmoothVelocity;
    private float _currentSpeed;
    private float _velocityY;
    private float _fireRate = 0.2f;
    private float _timer = 0;

    private Transform _cameraTransform;
    private CharacterController _controller;
    private BunnyCommands _bunnyCommands;

    bool lockCursor = false;

    void Start() {
        this._cameraTransform = Camera.main.transform;
        this._controller = this.GetComponent<CharacterController>();
        this._bunnyCommands = this.GetComponent<BunnyCommands>();
        this.spawn();
    }


    void Update() {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 inputDir = input.normalized;
        bool running = Input.GetKey(KeyCode.LeftShift);

        Move(inputDir, running);

        if (Input.GetAxisRaw("Jump") > 0)
            this.jump();

        if (Input.GetAxisRaw("Fire1") > 0)
            this.shoot();

        handleMouse();
    }

    private void shoot() {
        this._timer += Time.deltaTime;
        if (this._timer > this._fireRate) {
            this._bunnyCommands.Cmdshootpoop(this._cameraTransform.forward, this._controller.velocity);
            this._timer = 0;
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


    void jump() {
        if (_controller.isGrounded) {
            float jumpVelocity = Mathf.Sqrt(-2 * gravity * jumpHeight); // Kinnematik equation
            this._velocityY = jumpVelocity;
        }
    }

    //Controll player in air after jump
    float GetModifiedSmoothTime(float smoothTime) {
        if (_controller.isGrounded)
            return smoothTime;

        if (smoothTime == 0)
            return float.MaxValue;

        return smoothTime / airControlPercent;
    }

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

    private void spawn() {
        transform.position = new Vector3(Random.RandomRange(-40, 40),
                                         10,
                                         Random.RandomRange(-40, 40));
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.tag == "projectile") {
            this.spawn();
            Destroy(other.gameObject);
        }
    }
}
