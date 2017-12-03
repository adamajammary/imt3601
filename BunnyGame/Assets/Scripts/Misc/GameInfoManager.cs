using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameInfoManager : NetworkBehaviour {

    // Use this for initialization
    void Start () {
        if (this.isServer) {
            NetworkPlayerSelect lobby = Object.FindObjectOfType<NetworkPlayerSelect>();
            CmdInit(lobby.getGameMode(), lobby.getMap(), lobby.numPlayers);
        }
    }

    [Command]
    private void CmdInit(string gamemode, string map, int playerCount) {
        RpcInit(gamemode, map, playerCount);
    }

    [ClientRpc]
    private void RpcInit(string gamemode, string map, int playerCount) {
        init(gamemode, map, playerCount);
    }

    private void init(string gamemode, string map, int playerCount) {
        GameInfo.init(gamemode, map);
        StartCoroutine(setPlayersReady(playerCount));
        Debug.Log("INIT");
    }

    private IEnumerator setPlayersReady(int playerCount) {
        //Wait for all players to spawn, +1 for localplayer 
        while (playerCount != (GameObject.FindGameObjectsWithTag("Enemy").Length + 1))
            yield return 0;
        GameInfo.setPlayersToReady();
    }

    void OnApplicationQuit() {
        GameInfo.clear();
    }

    void OnDestroy() {
        GameInfo.clear();
    }
}