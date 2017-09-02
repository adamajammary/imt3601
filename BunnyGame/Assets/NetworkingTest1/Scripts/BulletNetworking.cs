using UnityEngine;

public class BulletNetworking : MonoBehaviour {
    [SerializeField] private int _attackPower = 10;

    // Use this for initialization
    private void Start () {
    }

    // Destroy the bullet on collision
    private void OnCollisionEnter(Collision collision) {
        PlayerNetworkingHealth healthScript = collision.gameObject.GetComponent<PlayerNetworkingHealth>();

        if (healthScript != null) {
            healthScript.TakeDamage(this._attackPower);
        }

        Destroy(this.gameObject);
    }

    // Update is called once per frame
    private void Update() {
    }
}
