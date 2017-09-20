using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerSelectCanvasEvents : NetworkBehaviour {

    private Button[]      _buttons;
    private NetworkClient _client = null;

    private void Start() {
        this._client  = NetworkClient.allClients[0];
        this._buttons = this.GetComponentsInChildren<Button>();

        // http://answers.unity3d.com/questions/908847/passing-a-temporary-variable-to-add-listener.html
        // NB! Don't pass in variable i to delegate, needs a temporary value.
        for (int i = 0; i < this._buttons.Length; i++) {
            int tempValue = i;
            this._buttons[i].onClick.RemoveAllListeners();
            this._buttons[i].onClick.AddListener(() => this.onClick(tempValue));  // NB!
        }

        this.onClick(0);
    }

    // Returns the unique identifier for the lobby player object instance.
    private uint getClientID() {
        uint id = 0;

        foreach (NetworkLobbyPlayer player in FindObjectsOfType<NetworkLobbyPlayer>()) {
            if (player.isLocalPlayer) {
                id = player.netId.Value;
                break;
            }
        }

        return id;
    }

    // Updates the model selection index based on which button the player clicked on.
    private void onClick(int model) {
        if ((model < 0) || (model >= this._buttons.Length)) { return; }

        for (int i = 0; i < this._buttons.Length; i++) {
            this._buttons[i].GetComponent<Image>().color = (i == model ? Color.yellow : Color.white);
        }

        this.SendPlayerSelectMessage(this.getClientID(), model);
    }

    // Create the player select message, and send it to the server.
    private void SendPlayerSelectMessage(uint clientID, int model) {
        PlayerSelectMessage message = new PlayerSelectMessage();

        message.clientID      = clientID;
        message.selectedModel = model;

        this.SendNetworkMessage(MetworkMessageType.MSG_PLAYERSELECT, message);
    }

    // Send the network message to the server.
    private void SendNetworkMessage(short messageType, MessageBase message) {
        if (this._client == null) { return; }

        switch (messageType) {
            case MetworkMessageType.MSG_PLAYERSELECT:
                this._client.Send(messageType, message);
                break;
            default:
                Debug.Log("ERROR! Unknown message type: " + messageType);
                break;
        }
    }
}
