using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

class PlayerInformation : NetworkBehaviour {
    [SyncVar]
    public int ConnectionID = -1;

    [SyncVar]
    public string playerName;

    private void Start() {
        if (this.isServer) CmdSpawnGI();
    }

    //Reason for putting this code here:
    // In order for commands to work, they need to be called with an object with authority.
    // In order to get authority on nonplayer objects, they have to be spawned by player objects with
    // the authority of a player.
    [Command]
    private void CmdSpawnGI() {
        StartCoroutine(spawnGI());
    }

    private IEnumerator spawnGI() {
        while (!this.connectionToClient.isReady) yield return 0;
        GameObject gimanager = Resources.Load<GameObject>("Prefabs/GameInfoManager");
        gimanager = Instantiate(gimanager);
        NetworkServer.SpawnWithClientAuthority(gimanager, this.connectionToClient);
    }
}
