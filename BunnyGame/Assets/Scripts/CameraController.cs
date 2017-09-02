using UnityEngine;

public class CameraController : MonoBehaviour {
    //public float      OffsetY       { get { return this._offsetY;       } set { this._offsetY       = value; } }
    //public GameObject Player        { get { return this._player;        } set { this._player        = value; } }
    //public float      RotationSpeed { get { return this._rotationSpeed; } set { this._rotationSpeed = value; } }

    [SerializeField]
    private float _offsetY = 5.0f;

    [SerializeField]
    private GameObject _player;

    [SerializeField]
    private float _rotationSpeed = 10.0f;

    private Vector3 _offset;

    // Use this for initialization
    private void Start() {
        this._offset    = (this.transform.position - this._player.transform.position);
        this._offset.y -= this._offsetY;
    }

    // Update is called once per frame
    private void Update() {
        float horizontal = (Input.GetAxis("Mouse X") * this._rotationSpeed);

        this._player.transform.Rotate(0, horizontal, 0);

        float      angle    = this._player.transform.eulerAngles.y;
        Quaternion rotation = Quaternion.Euler(0, angle, 0);

        this.transform.position = this._player.transform.position - (rotation * this._offset);
        this.transform.LookAt(this._player.transform);
    }
}
