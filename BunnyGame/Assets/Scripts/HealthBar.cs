using UnityEngine;

namespace BunnyGame {
public class HealthBar : MonoBehaviour {
    // Use this for initialization
    private void Start() {
    }

    // Make sure the health bar is always facing the camera independant of the players rotation
    private void Update() {
        transform.LookAt(Camera.main.transform);
    }
}
}