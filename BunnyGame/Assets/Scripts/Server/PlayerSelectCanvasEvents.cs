using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerSelectCanvasEvents : NetworkBehaviour {

    private Button[] _buttons;
    private InputField _nameInput;

    private void Start() {
        this._buttons = this.GetComponentsInChildren<Button>();
        this._nameInput = this.GetComponentInChildren<InputField>();

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

        for (int i = 0; i < this._buttons.Length; i++)
            this._buttons[i].GetComponent<Image>().color = (i == model ? Color.yellow : Color.white);

        this.SendPlayerSelectMessage(this.getClientID(), model);
    }

    public void onNameUpdate() {
        string name = this._nameInput.text.Trim();
        SendPlayerNameMessage(this.getClientID(), name);
    }

    // Create the player select message, and send it to the server.
    private void SendPlayerSelectMessage(uint clientID, int model) {
        PlayerSelectMessage message = new PlayerSelectMessage();

        message.clientID      = clientID;
        message.selectedModel = model;

        this.SendNetworkMessage(NetworkMessageType.MSG_PLAYERSELECT, message);
    }

    private void SendPlayerNameMessage(uint clientID, string name) {
        PlayerNameMessage message = new PlayerNameMessage() {
            clientID = clientID,
            name = name
        };
        this.SendNetworkMessage(NetworkMessageType.MSG_PLAYERNAME, message);
    }

    // Send the network message to the server.
    private void SendNetworkMessage(NetworkMessageType messageType, MessageBase message) {
        if (NetworkClient.allClients.Count < 1) { return; }

        switch (messageType) {
            case NetworkMessageType.MSG_PLAYERSELECT:
            case NetworkMessageType.MSG_PLAYERNAME:
                NetworkClient.allClients[0].Send((short)messageType, message);
                break;
            default:
                Debug.Log("ERROR! Unknown message type: " + messageType);
                break;
        }
    }
}
