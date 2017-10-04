using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddCollider : MonoBehaviour {
	// Use this for initialization
	void Awake () {
        foreach (Transform t in transform) {
            t.gameObject.AddComponent<MeshCollider>();
        }
    }
}
