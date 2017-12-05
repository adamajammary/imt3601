using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ServerSetup : NetworkBehaviour {
    // Use this for initialization
    void Start() {
        if (this.isServer) {
            StartCoroutine(init());
        }
    }

    private IEnumerator init() {
        GameObject player = null;
        do {
            player = GameObject.FindGameObjectWithTag("Player");
        } while (player == null);
        player.GetComponent<PlayerInformation>().spawnWithAuthority(Resources.Load<GameObject>("Prefabs/GameInfoManager"));

        while (!GameInfo.ready) yield return 0;

        if (GameInfo.gamemode == "Battleroyale") {
            GameObject fireWall = Resources.Load<GameObject>("Prefabs/FireWall");
            fireWall = Instantiate(fireWall);
            NetworkServer.Spawn(fireWall);
        } else if (GameInfo.gamemode == "Deathmatch") {
            GameObject deathmatchManager = Resources.Load<GameObject>("Prefabs/DeathmatchManager");
            deathmatchManager = Instantiate(deathmatchManager);
            NetworkServer.Spawn(deathmatchManager);
        }

        GameObject npcManager = Resources.Load<GameObject>("Prefabs/NPCManager");
        npcManager = Instantiate(npcManager);
        NetworkServer.Spawn(npcManager);

        GameObject island = Resources.Load<GameObject>("Prefabs/Islands/" + GameInfo.map);
        island = Instantiate(island);
        NetworkServer.Spawn(island);
    }
}
