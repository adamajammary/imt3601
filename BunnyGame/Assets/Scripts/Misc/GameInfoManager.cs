using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameInfoManager : NetworkBehaviour {

    string gamemode;
    string map;
    int numPlayers;

    // Use this for initialization
    void Start () {
        if (this.isServer) {
            NetworkPlayerSelect lobby = Object.FindObjectOfType<NetworkPlayerSelect>();
            this.gamemode = lobby.getGameMode();
            this.map = lobby.getMap();
            this.numPlayers = lobby.numPlayers;
            init(gamemode, map, numPlayers);
        } else {
            getServerData();
        }
    }

    public void sendDataToClients() {
        Debug.Log("GameInfoManager: Sending game info to clients.");
        RpcInit(this.gamemode, this.map, this.numPlayers);
    }

    private void getServerData() {
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInformation>().CmdGetGameInfo();
    }

    [ClientRpc]
    private void RpcInit(string gamemode, string map, int playerCount) {
        if (this.gamemode != gamemode || this.map != map || this.numPlayers != playerCount) {
            Debug.Log("GameInfoManager: Got game info from server.");
            this.gamemode = gamemode; this.map = map; this.numPlayers = playerCount;
            init(gamemode, map, playerCount);
        }
    }

    private void init(string gamemode, string map, int playerCount) {
        GameInfo.init(gamemode, map);
        StartCoroutine(setPlayersReady(playerCount));
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