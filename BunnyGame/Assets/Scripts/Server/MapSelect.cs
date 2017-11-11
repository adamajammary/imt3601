using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class MapSelect : MonoBehaviour {

    private Button[] _buttons;

    private void Start() {
        this._buttons = transform.GetComponentsInChildren<Button>();
    }

    // Send the network message to the server.
    public void sendMapMessage(string map) {
        if (NetworkClient.allClients.Count < 1)
            return;

  
        NetworkClient.allClients[0].Send((short)NetworkMessageType.MSG_MAP_SELECT, new StringMessage(map));

        foreach (Button button in this._buttons)
            button.GetComponent<Image>().color = (button.name == map ? Color.yellow : Color.white);
    }
}
