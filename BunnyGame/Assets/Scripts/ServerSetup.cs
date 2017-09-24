using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerSetup : NetworkBehaviour {

	// Use this for initialization
	void Start () {
		if (this.isServer) {
            GameObject fireWall = Resources.Load<GameObject>("Prefabs/FireWall");
            fireWall = Instantiate(fireWall);
            NetworkServer.Spawn(fireWall);
        }
	}
}
