using UnityEngine;

namespace BunnyGame {
public class AddCollider : MonoBehaviour {
    // Use this for initialization
    private void Start() {
        foreach (Transform childTransform in this.transform) {
            childTransform.gameObject.AddComponent<MeshCollider>();
        }
    }
}
}
