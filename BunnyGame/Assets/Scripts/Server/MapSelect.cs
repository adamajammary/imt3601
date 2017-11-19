using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class MapSelect : NetworkBehaviour {

    private Button[] _buttons;

    private void OnEnable() {
        this._buttons = transform.GetComponentsInChildren<Button>();

        foreach (Button button in this._buttons) {
            button.GetComponent<Image>().color         = Color.white;
            button.GetComponentInChildren<Text>().text = (button.name + ": 0");
        }

        StartCoroutine(this.registerNetworkHandlers(1.0f));
    }

    public IEnumerator registerNetworkHandlers(float seconds) {
        yield return new WaitForSeconds(seconds);
        this.registerNetworkHandlers();
    }

    // Registers the network message types that this client will listen to.
    private void registerNetworkHandlers() {
        if ((NetworkClient.allClients.Count > 0) && !NetworkClient.allClients[0].connection.CheckHandler((short)NetworkMessageType.MSG_MAP_VOTE))
            NetworkClient.allClients[0].RegisterHandler((short)NetworkMessageType.MSG_MAP_VOTE, recieveVoteMessage);
    }

    // Send the network message to the server.
    public void sendMapMessage(string map) {
        if (NetworkClient.allClients.Count < 1)
            return;

        this.registerNetworkHandlers();
        NetworkClient.allClients[0].Send((short)NetworkMessageType.MSG_MAP_SELECT, new StringMessage(map));

        // Update UI
        foreach (Button button in this._buttons)
            button.GetComponent<Image>().color = (button.name == map ? Color.yellow : Color.white);
    }

    private void recieveVoteMessage(NetworkMessage message) {
        string msg = message.ReadMessage<StringMessage>().value;
        Dictionary<string, int> votes = new Dictionary<string, int>();

        foreach (string vote in msg.Split('|')) {
            if (vote.Contains(":")) {
                string map = vote.Split(':')[0];
                int voteCount = int.Parse(vote.Split(':')[1]);

                votes.Add(map, voteCount);
            }
        }       

        // Update UI
        foreach (Button button in this._buttons) {
            if (votes.ContainsKey(button.name))
                button.GetComponentInChildren<Text>().text = button.name + ": " + votes[button.name];
            else
                button.GetComponentInChildren<Text>().text = button.name + ": 0";
        }
    }
}
