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

            GameObject npcManager = Resources.Load<GameObject>("Prefabs/NPCManager");
            npcManager = Instantiate(npcManager);
            NetworkServer.Spawn(npcManager);

            NetworkPlayerSelect lobby = Object.FindObjectOfType<NetworkPlayerSelect>();
            string map = lobby.getMap();
            GameObject island = Resources.Load<GameObject>("Prefabs/Islands/" + map);
            island = Instantiate(island);
            NetworkServer.Spawn(island);
        }
	}
}
