using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public GameObject playerPrefab;

    private Dictionary<string, GameObject> players;
    private client c;
	
    public void init(client c, string playerNames) {
        DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene("networking");
        this.c = c;

        string[] pn = playerNames.Split('|');
        this.players = new Dictionary<string, GameObject>();
        for (int i = 1; i < pn.Length; i++) {
            Debug.Log("Adding player " + pn[i]);
            GameObject player = Instantiate(playerPrefab);
            player.name = pn[i];
            DontDestroyOnLoad(player);
            this.players.Add(player.name, player);
        }        
    }
	
	// Update is called once per frame
	void Update () {
        if (this.c != null) {
            while (this.c.input.Count > 0) {
                string data = c.input.Dequeue();
                string[] d = data.Split('|');
                this.players[d[1]].GetComponent<testController>().handleInput(d[2]);
            }
        }
	}
}
