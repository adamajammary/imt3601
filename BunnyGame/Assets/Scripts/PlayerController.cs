using UnityEngine;

public class PlayerController : MonoBehaviour {
    public float JumpForce { get { return this._jumpForce; } set { this._jumpForce = value; } }
    public float Speed     { get { return this._speed; }     set { this._speed = value; } }

    [SerializeField]
    private float _jumpForce;

    [SerializeField]
    private float _speed;

    private bool      _isJumping;
    private Rigidbody _rb;

    // Use this for initialization
    void Start() {
        this._isJumping = false;
        this._rb        = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision col) {
        if ((col.gameObject.tag == "Bush") || (col.gameObject.tag == "Ground") || (col.gameObject.tag == "Hill")) {
            this._isJumping = false;
        }
        print("PlayerController::OnCollisionEnter: " + col.gameObject.tag);
    }

    // Update is called once per frame
    void Update() {
        this.MovePlayer();
    }

    // Takes keyboard input to move player
    void MovePlayer() {
        //Vector3 translate = new Vector3(-Input.GetAxis("Horizontal"), 0.0f, -Input.GetAxis("Vertical"));
        Vector3 translate = new Vector3(0.0f, 0.0f, -Input.GetAxis("Vertical"));
        this.transform.Translate(translate * this.Speed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space) && !this._isJumping) {
            this._rb.AddForce(new Vector3(0.0f, (this.Speed * this.JumpForce), 0.0f));
            this._isJumping = true;
        }
    }
}
