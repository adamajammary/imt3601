using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerSetup : NetworkBehaviour {
	// Use this for initialization
	void Start () {
		if (this.isServer) {
            if (GameInfo.gamemode == "Battleroyale") {
                GameObject fireWall = Resources.Load<GameObject>("Prefabs/FireWall");
                fireWall = Instantiate(fireWall);
                NetworkServer.Spawn(fireWall);
            }

            GameObject npcManager = Resources.Load<GameObject>("Prefabs/NPCManager");
            npcManager = Instantiate(npcManager);
            NetworkServer.Spawn(npcManager);

            GameObject island = Resources.Load<GameObject>("Prefabs/Islands/" + GameInfo.map);
            island = Instantiate(island);
            NetworkServer.Spawn(island);
        }
	}
}
