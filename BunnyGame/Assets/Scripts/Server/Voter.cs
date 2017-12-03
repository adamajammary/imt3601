using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public abstract class Voter : NetworkBehaviour {

    protected Button[] _buttons;

    private void OnEnable() {
        this._buttons = transform.GetComponentsInChildren<Button>();

        foreach (Button button in this._buttons) {
            button.GetComponent<Image>().color = Color.white;
            button.GetComponentInChildren<Text>().text = (button.name + ": 0");
        }

        StartCoroutine(registerNetworkHandlers());
    }

    public abstract IEnumerator registerNetworkHandlers();
    // Send the network message to the server.
    public abstract void sendVote(string vote);

    protected void recieveVote(NetworkMessage message) {
        string msg = message.ReadMessage<StringMessage>().value;
        Dictionary<string, int> votes = new Dictionary<string, int>();

        foreach (string vote in msg.Split('|')) {
            if (vote.Contains(":")) {
                string map = vote.Split(':')[0];
                int voteCount = int.Parse(vote.Split(':')[1]);

                votes.Add(map, voteCount);
            }
        }
        recieveGfxUpdate(votes);
    }

    protected void sendGfxUpdate(string vote) {
        // Update UI
        foreach (Button button in this._buttons)
            button.GetComponent<Image>().color = (button.name == vote ? Color.yellow : Color.white);
    }

    protected void recieveGfxUpdate(Dictionary<string, int> votes) {
        // Update UI
        foreach (Button button in this._buttons) {
            if (votes.ContainsKey(button.name))
                button.GetComponentInChildren<Text>().text = button.name + ": " + votes[button.name];
            else
                button.GetComponentInChildren<Text>().text = button.name + ": 0";
        }
    }   
}
