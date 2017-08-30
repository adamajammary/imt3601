using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float groundPositionY;
    public float jumpForce;
    public float speed;

    private bool      isJumping;
    private Rigidbody rb;

    // Use this for initialization
    void Start()
    {
        this.isJumping = false;
        this.rb        = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == ("Ground")) {
            this.isJumping = false;

        }
        print(col.gameObject.tag);
    }

    // Update is called once per frame
    void Update()
    {
        this.MovePlayer();
    }

    // Takes keyboard input to move player
    void MovePlayer()
    {
        Vector3 translate = new Vector3(-Input.GetAxis("Horizontal"), 0.0f, -Input.GetAxis("Vertical"));
        this.transform.Translate(translate * this.speed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space) && !this.isJumping) {
            this.rb.AddForce(new Vector3(0.0f, (this.speed * this.jumpForce), 0.0f));
            this.isJumping = true;

            print(this.isJumping);
        }
    }
}
