//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SetUpLocalPlayer : NetworkBehaviour {

    // Use this for initialization
    void Start () {
        if (this.isLocalPlayer) {
            ThirdPersonCamera camera = FindObjectOfType<ThirdPersonCamera>();
            camera.SetTarget(this.transform);
            this.tag = "Player";
            transform.GetChild(0).gameObject.SetActive(true);
        } else
            this.tag = "Enemy";
        
    }
}
