using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInfoManager : MonoBehaviour {

	// Use this for initialization
	void Awake () {
        NetworkPlayerSelect lobby = Object.FindObjectOfType<NetworkPlayerSelect>();
        GameInfo.init(lobby.getGameMode(), lobby.getMap());
        StartCoroutine(setPlayersReady(lobby.numPlayers));
    }
	
    private IEnumerator setPlayersReady(int playerCount) {
        //Wait for all players to spawn, +1 for localplayer 
        while (playerCount != (GameObject.FindGameObjectsWithTag("Enemy").Length + 1))
            yield return 0;
        GameInfo.setPlayersToReady();
    }


    //It's important to stop the NPCThread when quitting
    void OnApplicationQuit() {
        NPCWorldView.clear();
    }

    void OnDestroy() {
        NPCWorldView.clear();
    }
}