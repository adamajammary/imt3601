using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerSetup : NetworkBehaviour {
    [SyncVar(hook = "SpawnIsland")]
    string island;
	// Use this for initialization
	void Start () {
        island = "";
		if (this.isServer) {
            GameObject fireWall = Resources.Load<GameObject>("Prefabs/FireWall");
            fireWall = Instantiate(fireWall);
            NetworkServer.Spawn(fireWall);

            GameObject npcManager = Resources.Load<GameObject>("Prefabs/NPCManager");
            npcManager = Instantiate(npcManager);
            NetworkServer.Spawn(npcManager);

            StartCoroutine(hack());
        }
	}

    private IEnumerator hack() {
        island = "hack";
        yield return new WaitForSeconds(0.2f);
        string[] islands = { "Island", "Island42" };
        island = islands[Random.Range(0, islands.Length)];
    }

    void SpawnIsland(string name) {
        this.island = name;
        if (this.island == "hack" || this.island == "") return;
        GameObject island = Resources.Load<GameObject>("Prefabs/Islands/" + name);
        Instantiate(island);
    }
}
