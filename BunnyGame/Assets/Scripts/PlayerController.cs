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
    public float airControlPercent;

    public float turnSmoothTime = 0.2f;
    public float speedSmoothTime = 0.2f;

    public bool insideWall;

    private const float _damageRate = 0.25f;    //How often to damage player outside wall
    private float _damageTimer;                 //Timer used to find out when to damage player    

    private float _turnSmoothVelocity;
    private float _speedSmoothVelocity;
    private float _currentSpeed;
    private float _velocityY;

    private float _maxFallSpeed = 20; // How fast you can fall before starting to take fall damage
    private int _fallDamage = 40;
    private bool _dealFallDamageOnCollision = false;
    private bool _fallDamageImmune = false;

    private Transform _cameraTransform;
    private CharacterController _controller;

    bool lockCursor = false;

    public void onWaterStay(float waterForce) {
        this._velocityY += waterForce * Time.deltaTime;
    }

    void Start() {
        if (!this.isLocalPlayer) { return; }

        this._cameraTransform = Camera.main.transform;
        this._controller = this.GetComponent<CharacterController>();

        this.airControlPercent = 1;

        this._damageTimer = 0;
        this.insideWall = true;
    }


    void Update() {
        if (!this.insideWall) //Feels hacky, but when TakeDamage only works on the server its got to be this way
            wallDamage();
        if (!this.isLocalPlayer) { return; }

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 inputDir = input.normalized;
        bool running = Input.GetKey(KeyCode.LeftShift);

        Move(inputDir, running);

        if (Input.GetAxisRaw("Jump") > 0)
            this.jump();

        handleFallDamage();
        HandleAiming();

        handleMouse();
    }

    // Turn off and on MeshRenderer so FPS camera works
    private void HandleAiming(){
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            foreach (Transform t in this.gameObject.transform.GetChild(1))
            {
                t.gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
        }
        else if(Input.GetKeyUp(KeyCode.Mouse1))
        {
            foreach (Transform t in this.gameObject.transform.GetChild(1))
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



    void jump() {
        if (_controller.isGrounded) {
            float jumpVelocity = Mathf.Sqrt(-2 * gravity * jumpHeight); 
            this._velocityY = jumpVelocity;
        }
    }

    private void handleFallDamage()
    {
        if (-this._velocityY > _maxFallSpeed && !_dealFallDamageOnCollision && !_fallDamageImmune)
            _dealFallDamageOnCollision = true;
        else if (-this._velocityY < 1 && _dealFallDamageOnCollision) {
            this.GetComponent<PlayerHealth>().TakeDamage(_fallDamage);
            _dealFallDamageOnCollision = false;
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

    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void wallDamage() {
        if (this._damageTimer > _damageRate) {
            this.GetComponent<PlayerHealth>().TakeDamage(1);
            this._damageTimer = 0;
        }   
        this._damageTimer += Time.deltaTime;
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.tag == "projectile") {
            this.GetComponent<PlayerHealth>().TakeDamage(other.gameObject.GetComponent<BunnyPoop>().GetDamage());
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "foxbite" && other.transform.parent != transform) {
            this.GetComponent<PlayerHealth>().TakeDamage(other.GetComponentInParent<FoxController>().getDamage());
        }
        else if (other.gameObject.name == "Water") {
            this._fallDamageImmune = true; // Immune from falldamage when in water
            Debug.Log("in water");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Water") {
            this._fallDamageImmune = false;
            Debug.Log("out of water");
        }
    }
}
