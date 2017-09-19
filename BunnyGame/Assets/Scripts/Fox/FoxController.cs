using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FoxController : NetworkBehaviour {

	// Use this for initialization
	void Start () {
        if (!this.isLocalPlayer) { return; }

        PlayerController playerController = GetComponent<PlayerController>();
        playerController.runSpeed = 15;
	}
	
	// Update is called once per frame
	void Update () {
        if (!this.isLocalPlayer) { return; }

    }
}
