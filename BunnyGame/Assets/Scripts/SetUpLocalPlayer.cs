﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SetUpLocalPlayer : NetworkBehaviour {

	// Use this for initialization
	void Start () {
        if (this.isLocalPlayer) {
            this.gameObject.AddComponent<PlayerController>();
            ThirdPersonCamera camera = FindObjectOfType<ThirdPersonCamera>();
            camera.SetTarget(this.transform);
        } else {
            this.gameObject.layer = 11; //enemy layer
        }
	}
	
}
