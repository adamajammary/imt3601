using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SetUpLocalPlayer : NetworkBehaviour {
    NetworkTransformChild modelNetworkTransform;
	// Use this for initialization
	void Start () {
        if (this.isLocalPlayer) {
            this.gameObject.AddComponent<PlayerController>();
            ThirdPersonCamera camera = FindObjectOfType<ThirdPersonCamera>();
            camera.SetTarget(this.transform);
            this.tag = "Player";
        } else
            this.tag = "Enemy";

        setupClass();

    }

    void setupClass() {

        // Temporarily choosing class at random
        // NB! This random class isn't synced across clients...
        int r = Random.Range(0, 2);
        switch (r) {
            case 0:
                setupFox();
                break;
            case 1:
                setupBunny();
                break;
        }
    }

    private void loadModel(string name) {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/"+name);
        GameObject model = Instantiate(prefab);
        model.transform.SetParent(transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.rotation = transform.rotation;
   
    }

    // Setup the player as a Fox
    private void setupFox() {
        loadModel("FoxModel");

        if (!this.isLocalPlayer)
            return;
        // Setup fox-specific attributes, abilities, etc...
        PlayerController pc = GetComponent<PlayerController>();
        pc.runSpeed = 15;


    }

    // Setup the player as a bunny
    private void setupBunny() {
        loadModel("BunnyModel");

        if (!this.isLocalPlayer)
            return;
        // Set up bunny-specific attributes, abilities, etc...
        PlayerController pc = GetComponent<PlayerController>();
        pc.jumpHeight = 3;
    }

}
