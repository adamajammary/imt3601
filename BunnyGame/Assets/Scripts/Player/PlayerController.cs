using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class PlayerController : NetworkBehaviour {

    public List<SpecialAbility> abilities = new List<SpecialAbility>();

    public float walkSpeed = 5;
    public float runSpeed = 12;
    public float gravity = -12;
    public float jumpHeight = 3;

    [Range(0, 1)]
    public float airControlPercent;

    public float turnSmoothTime = 0.2f;
    public float speedSmoothTime = 0.2f;

    public float currentSpeed;
    public float velocityY;

    private bool _CC = false; //Turns off players ability to control character, used for CC effects

    private float _turnSmoothVelocity;
    private float _speedSmoothVelocity;


    private Transform _cameraTransform;
    public CharacterController controller;
    private PlayerEffects playerEffects;
    
    public bool running = false;

    private bool _moveDirectionLocked = false;
    private float _targetRotation = 0;


    void Start() {
		CorrectRenderingMode(); // Calling this here to fix the rendering order of the model, because materials have rendering mode fade


        if (!this.isLocalPlayer)
            return;
        
        this._cameraTransform = Camera.main.transform;
        this.controller = this.GetComponent<CharacterController>();
        this.playerEffects = GetComponent<PlayerEffects>();

        this.airControlPercent = 1;
	}

    void Update() {
        if (!this.isLocalPlayer) // NB! wallDamage should now work on clients
            return;

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 inputDir = input.normalized;
        running = Input.GetKey(KeyCode.LeftShift);
        _moveDirectionLocked = Input.GetKey(KeyCode.LeftAlt);

        handleSpecialAbilities();

        if (!this._CC) {
            Move(inputDir);
            if (Input.GetKeyDown(KeyCode.Space))
                this.jump();
        }

        HandleAiming();
    }

    public bool getGrounded() {
        return controller.isGrounded;
    }

    public void setCC(bool value) {
        this._CC = value;
        if (value) {
            this.currentSpeed = 0;
            this.velocityY = 0;
        }
    }

    public bool getCC() {
        return this._CC;
    }

    // Turn off and on MeshRenderer so FPS camera works
    private void HandleAiming(){
        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            foreach (Transform t in this.gameObject.transform.GetChild(1)) {
                if(t.gameObject.GetComponent<MeshRenderer>() != null)
                    t.gameObject.GetComponent<MeshRenderer>().enabled = false;
                else if(t.gameObject.GetComponent<SkinnedMeshRenderer>() != null)
                    t.gameObject.GetComponent<SkinnedMeshRenderer>().enabled = false;
            }
        }
        else if(Input.GetKeyUp(KeyCode.Mouse1)) {
            foreach (Transform t in this.gameObject.transform.GetChild(1)) {
                if (t.gameObject.GetComponent<MeshRenderer>() != null)
                    t.gameObject.GetComponent<MeshRenderer>().enabled = true;
                else if (t.gameObject.GetComponent<SkinnedMeshRenderer>() != null)
                    t.gameObject.GetComponent<SkinnedMeshRenderer>().enabled = true;
            }
        }
    }

    private void handleSpecialAbilities() {
        for (int i = 0; i < abilities.Count && i < 9; i++) {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) {
                StartCoroutine(abilities[i].useAbility());
            }
        }
    }

    public void Move(Vector2 inputDir) {
        if(!_moveDirectionLocked)
            _targetRotation = _cameraTransform.eulerAngles.y;

        transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation,
                                                    ref _turnSmoothVelocity, GetModifiedSmoothTime(turnSmoothTime));

        float targetSpeed = ((running) ? runSpeed : walkSpeed) * inputDir.magnitude;
        this.currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref _speedSmoothVelocity, GetModifiedSmoothTime(speedSmoothTime));

        this.velocityY += Time.deltaTime * gravity;

        Vector3 moveDir = transform.TransformDirection(new Vector3(inputDir.x, 0, inputDir.y));
        moveDir.y = 0;

        Vector3 velocity = moveDir.normalized * currentSpeed * playerEffects.getSpeed() + Vector3.up * velocityY;


        this.controller.Move(velocity * Time.deltaTime);

        if (controller.isGrounded)
            velocityY = 0;
    }

    public void jump() {
        if (controller.isGrounded && !onWall()) { 
            float jumpVelocity = Mathf.Sqrt(-2 * gravity * jumpHeight * this.playerEffects.getJump()); 
            this.velocityY = jumpVelocity;
        }
    }

    //Controll player in air after jump
    float GetModifiedSmoothTime(float smoothTime) {
        if (controller.isGrounded)
            return smoothTime;

        if (smoothTime == 0)
            return float.MaxValue;

        return smoothTime / airControlPercent;
    }

    private void OnDestroy() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

	public void CorrectRenderingMode() {
		Material[] materials;

        foreach (Transform child in this.transform.GetChild(1)) {
            if (child.gameObject.GetComponent<Renderer>() != null)
                materials = child.gameObject.GetComponent<Renderer>().materials;
            else if (child.gameObject.GetComponent<SkinnedMeshRenderer>() != null)
                materials = child.gameObject.GetComponent<SkinnedMeshRenderer>().materials;
            else
                continue;

            foreach (Material mat in materials) {
                mat.SetInt("_ZWrite", 1);
                mat.renderQueue = 2000;
            }
        }
    }

    private bool onWall() {
        const float deltaLimit = 1.00f;
        Vector3[] offsets = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

        float[] distances = new float[offsets.Length];
        RaycastHit hit = new RaycastHit();

        for (int i = 0; i < offsets.Length; i++) {
            Physics.Raycast(transform.position + offsets[i], Vector3.down, out hit);
            distances[i] = hit.distance;
        }

        foreach (var dist in distances) {
            foreach (var dist2 in distances) {
                if (Mathf.Abs(dist - dist2) > deltaLimit) {
                    return true;
                }

            }
        }
        return false;
    }
}
