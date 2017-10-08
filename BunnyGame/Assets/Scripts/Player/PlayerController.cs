using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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

    private float _turnSmoothVelocity;
    private float _speedSmoothVelocity;


    private Transform _cameraTransform;
    public CharacterController controller;
    private EscMenu escButtonPress;

    bool lockCursor = true;
    bool escMenu = false;
    public bool running = false;

    void Start() {
		CorrectRenderingMode(); // Calling this here to fix the rendering order of the model, because materials have rendering mode fade


        if (!this.isLocalPlayer)
            return;

        this.escButtonPress = FindObjectOfType<EscMenu>();
        this._cameraTransform = Camera.main.transform;
        this.controller = this.GetComponent<CharacterController>();

        this.airControlPercent = 1;
	}

    void Update() {
        if (!this.isLocalPlayer) // NB! wallDamage should now work on clients
            return;

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 inputDir = input.normalized;
        running = Input.GetKey(KeyCode.LeftShift);

        handleSpecialAbilities();
        Move(inputDir);

        if (Input.GetAxisRaw("Jump") > 0)
            this.jump();

        HandleAiming();
        handleMouse();
        handleEscMenu();
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
        float targetRotation = _cameraTransform.eulerAngles.y;
        transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation,
                                                    ref _turnSmoothVelocity, GetModifiedSmoothTime(turnSmoothTime));

        float targetSpeed = ((running) ? runSpeed : walkSpeed) * inputDir.magnitude;
        this.currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref _speedSmoothVelocity, GetModifiedSmoothTime(speedSmoothTime));

        this.velocityY += Time.deltaTime * gravity;

		Vector3 moveDir = _cameraTransform.TransformDirection(new Vector3(inputDir.x, 0, inputDir.y));
        moveDir.y = 0;
        
        Vector3 velocity = moveDir.normalized * currentSpeed + Vector3.up * velocityY;


        this.controller.Move(velocity * Time.deltaTime);

        if (controller.isGrounded)
            velocityY = 0;
    }

    public void jump() {
        if (controller.isGrounded) {
            float jumpVelocity = Mathf.Sqrt(-2 * gravity * jumpHeight); 
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

    private void handleEscMenu()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            escMenu = !escMenu;
          
        escButtonPress.EscPress(escMenu);

        if(escButtonPress.resumePressed())
        {
            escMenu = false;
            escButtonPress.rusumePressedReset();
            lockCursor = true;
        }  
    }
}
