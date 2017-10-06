using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;

public class PlayerSelectCanvasEvents : NetworkBehaviour {

    private Button[]      _buttons;
    private NetworkClient _client;
    private InputField    _nameInput;

    private void Start() {
        this._client    = NetworkClient.allClients[0];
        this._buttons   = this.GetComponentsInChildren<Button>();
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

    // Updates the model selection index based on which button the player clicked on.
    private void onClick(int model) {
        if ((model < 0) || (model >= this._buttons.Length)) { return; }

        for (int i = 0; i < this._buttons.Length; i++)
            this._buttons[i].GetComponent<Image>().color = (i == model ? Color.yellow : Color.white);

        this.SendPlayerSelectMessage(model);
    }

    public void onNameUpdate() {
        this.SendPlayerNameMessage(this._nameInput.text.Trim());
    }

    // Create the player select message, and send it to the server.
    private void SendPlayerSelectMessage(int model) {
        this.SendNetworkMessage(NetworkMessageType.MSG_PLAYERSELECT, new IntegerMessage(model));
    }

    private void SendPlayerNameMessage(string name) {
        this.SendNetworkMessage(NetworkMessageType.MSG_PLAYERNAME, new StringMessage(name));
    }

    // Send the network message to the server.
    private void SendNetworkMessage(NetworkMessageType messageType, MessageBase message) {
        if (this._client == null) { return; }

        switch (messageType) {
            case NetworkMessageType.MSG_PLAYERSELECT:
            case NetworkMessageType.MSG_PLAYERNAME:
                this._client.Send((short)messageType, message);
                break;
            default:
                Debug.Log("ERROR! Unknown message type: " + messageType);
                break;
        }
    }
}
