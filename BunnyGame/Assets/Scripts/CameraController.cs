using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector3    offset;
    public  float      offsetY;
    public  GameObject player;
    public  float      rotationSpeed;

    // Use this for initialization
    private void Start()
    {
        this.offset    = (this.transform.position - this.player.transform.position);
        this.offset.y -= offsetY;
    }

    // Update is called once per frame
    private void Update()
    {
        float horizontal = (Input.GetAxis("Mouse X") * this.rotationSpeed);

        this.player.transform.Rotate(0, horizontal, 0);

        float      angle    = this.player.transform.eulerAngles.y;
        Quaternion rotation = Quaternion.Euler(0, angle, 0);

        this.transform.position = this.player.transform.position - (rotation * this.offset);
        this.transform.LookAt(this.player.transform);
    }
}
