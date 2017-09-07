using UnityEngine;
using UnityEngine.Networking;

namespace BunnyGame {
public class PlayerController : NetworkBehaviour {
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform  _bulletSpawn;
    [SerializeField] private float      _bulletVelocity = 5.0f;
    [SerializeField] private float      _jumpForce      = 50.0f;
    [SerializeField] private float      _movementSpeed  = 10.0f;
    [SerializeField] private float      _rotationSpeed  = 150.0f;
    [SerializeField] private Vector3    _cameraOffset;

    private bool      _isJumping;
    private Rigidbody _rb;
    
    // Use this for initialization
    void Start() {
        if (!this.isLocalPlayer) { return; }

        this._isJumping = false;
        this._rb        = this.GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision col) {
        if ((col.gameObject.tag == "Bush") || (col.gameObject.tag == "Ground") || (col.gameObject.tag == "Hill")) {
            this._isJumping = false;
        }
        //print("PlayerController::OnCollisionEnter: " + col.gameObject.tag);
    }
    
    //public override void OnStartLocalPlayer() {
    //}

    // Update is called once per frame
    private void Update() {
        if (!this.isLocalPlayer) { return; }

        this.rotate();

        if (Input.GetButton("Horizontal") || Input.GetButton("Vertical")) {
            this.movePlayer();
        }

        if (Input.GetButtonDown("Jump")) {
            this.jump();
        }

        if (Input.GetButtonDown("Fire1")) {
            this.CmdFire();
        }
    }

    // Create, fire, spawn and destroy a bullet instance
    // Command: sends command from client to server => invoked on the server
    [Command]
    private void CmdFire() {
        GameObject bullet = Instantiate(this._bulletPrefab, this._bulletSpawn.position, this._bulletSpawn.rotation);

        bullet.GetComponent<Rigidbody>().velocity = (bullet.transform.forward * (this._bulletVelocity + this._movementSpeed));
        NetworkServer.Spawn(bullet);
        Destroy(bullet, 2.0f);
    }

    // Takes keyboard input to make the player jump
    private void jump() {
        if (this._isJumping) { return; }

        this._rb.AddForce(new Vector3(0.0f, (this._movementSpeed * this._jumpForce), 0.0f));
        this._isJumping = true;
    }

    // Takes keyboard input to move player
    private void movePlayer() {
        Vector3 translate = new Vector3(0.0f, 0.0f, -Input.GetAxis("Vertical"));
        this.transform.Translate(translate * this._movementSpeed * Time.deltaTime);
    }
    
    // Rotates the player and the camera based on the player orientation
    private void rotate() {
        // Rotate the player
        float horizontal = ((Input.GetAxis("Mouse X") + Input.GetAxis("Horizontal")) * this._rotationSpeed * Time.deltaTime);
        this.transform.Rotate(new Vector3(0.0f, horizontal, 0.0f));
        
        // Make the camera follow the player
        Quaternion rotation = Quaternion.Euler(0.0f, this.transform.eulerAngles.y, 0.0f);
        Camera.main.transform.position = (this.transform.position + (rotation * this._cameraOffset));
        Camera.main.transform.LookAt(this.transform);
    }
}
}
