using UnityEngine;
using UnityEngine.Networking;

public class PlayerNetworkingController : NetworkBehaviour {
    //public GameObject BulletPrefab  { get { return this._bulletPrefab;  } set { this._bulletPrefab  = value; } }
    //public float      MovementSpeed { get { return this._movementSpeed; } set { this._movementSpeed = value; } }
    //public float      RotationSpeed { get { return this._rotationSpeed; } set { this._rotationSpeed = value; } }

    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform  _bulletSpawn;
    [SerializeField] private float      _bulletVelocity = 6.0f;
    [SerializeField] private float      _movementSpeed  = 3.0f;
    [SerializeField] private float      _rotationSpeed  = 150.0f;

    // Use this for initialization
    private void Start() {
    }

    // Set the color of your client player
    public override void OnStartLocalPlayer() {
        this.GetComponent<MeshRenderer>().material.color = Color.blue;
    }

    // Update is called once per frame
    private void Update() {
        if (!this.isLocalPlayer) { return; }

        if (Input.GetButton("Horizontal") || Input.GetButton("Vertical")) {
            this.transformPlayer();
        }

        if (Input.GetButtonDown("Fire1")) {
            this.CmdFire();
        }
    }

    // Create, fire, spawn and destroy a bullet instance
    [Command]
    private void CmdFire() {
        GameObject bullet = Instantiate(this._bulletPrefab, this._bulletSpawn.position, this._bulletSpawn.rotation);

        bullet.GetComponent<Rigidbody>().velocity = (bullet.transform.forward * this._bulletVelocity);
        NetworkServer.Spawn(bullet);
        Destroy(bullet, 2.0f);
    }

    // Transforms (moves/translates, rotates and scales) player
    private void transformPlayer() {
        float horizontal = (Input.GetAxis("Horizontal") * Time.deltaTime * this._rotationSpeed);
        float vertical   = (Input.GetAxis("Vertical")   * Time.deltaTime * this._movementSpeed);

        this.transform.Rotate(0.0f, horizontal, 0.0f);
        this.transform.Translate(0.0f, 0.0f, vertical);
    }
}
