using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SetUpLocalPlayer : NetworkBehaviour {

	// Use this for initialization
	void Start () {
        setupModel();


        if (this.isLocalPlayer) {
            this.gameObject.AddComponent<PlayerController>();
            ThirdPersonCamera camera = FindObjectOfType<ThirdPersonCamera>();
            camera.SetTarget(this.transform);
            this.tag = "Player";
        } else
            this.tag = "Enemy";
	}

    void setupModel() {
        GameObject model = Instantiate(Resources.Load<GameObject>("Prefabs/BunnyModel"));
        model.transform.SetParent(transform);
    }
	
}
