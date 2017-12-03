using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameInfoManager : NetworkBehaviour {
    [SyncVar]
    string _map;
    [SyncVar]
    string _gamemode;
    [SyncVar]
    int _playerCount;

    // Use this for initialization
    void Start () {
        Debug.Log("DSADS");
        if (this.isServer) {
            Debug.Log("SHIT");
            NetworkPlayerSelect lobby = Object.FindObjectOfType<NetworkPlayerSelect>();
            _map = lobby.getMap();
            _gamemode = lobby.getGameMode();
            _playerCount = lobby.numPlayers;
        }
        StartCoroutine(init());
    }

    private IEnumerator init() {
        yield return new WaitForSeconds(0.5f);
        init(_gamemode, _map, _playerCount);
    }

    private void init(string gamemode, string map, int playerCount) {
        GameInfo.init(gamemode, map);
        StartCoroutine(setPlayersReady(playerCount));
    }

    private IEnumerator setPlayersReady(int playerCount) {
        Debug.Log(playerCount);
        //Wait for all players to spawn, +1 for localplayer 
        while (playerCount != (GameObject.FindGameObjectsWithTag("Enemy").Length + 1))
            yield return 0;
        GameInfo.setPlayersToReady();
        Debug.Log("CUUCK");
    }

    void OnApplicationQuit() {
        GameInfo.clear();
    }

    void OnDestroy() {
        GameInfo.clear();
    }
}