using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

class PlayerInformation : NetworkBehaviour {
    [SyncVar]
    public int ConnectionID = -1;

    [SyncVar]
    public string playerName;

    /**
     * Takes a non instance GameObject and spawns it with
     * the authority of itself. 
     */ 
    public void spawnWithAuthority(GameObject obj) {
        CmdSpawn(obj);
    }

    //Reason for putting this code here:
    // In order for commands to work, they need to be called with an object with authority.
    // In order to get authority on nonplayer objects, they have to be spawned by player objects with
    // the authority of a player.
    [Command]
    private void CmdSpawn(GameObject obj) {
        StartCoroutine(spawn(obj));
    }

    private IEnumerator spawn(GameObject obj) {
        while (!this.connectionToClient.isReady) yield return 0;
        obj = Instantiate(obj);
        NetworkServer.SpawnWithClientAuthority(obj, this.connectionToClient);
    }

    [Command]
    public void CmdGetGameInfo() {
        GameObject.FindObjectOfType<GameInfoManager>().sendDataToClients();
    }

    [Command]
    public void CmdRespawnNPC(GameObject npc) {
        GameObject.FindObjectOfType<NPCManager>().respawnNPC(npc);
    }
}
