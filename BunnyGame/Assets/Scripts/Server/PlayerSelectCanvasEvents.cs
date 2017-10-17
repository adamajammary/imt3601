using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;

public class PlayerSelectCanvasEvents : NetworkBehaviour {

    public Button[]   _buttons;
    public InputField _nameInput;
    private Text       _nameAvailableText;

    // This function is called when the object becomes enabled and active.
    // ISSUE #67: When the player re-connects in the lobby manager, the GUI is not refreshed if it re-uses the last connection.
    // SOLUTION:  Reset input text and button selections whenever the canvas object is re-enabled (re-connected).
    private void OnEnable() {
        this._buttons           = this.transform.parent.GetChild(3).GetComponentsInChildren<Button>();
        this._nameInput         = this.transform.parent.GetChild(2).GetComponent<InputField>();
        this._nameAvailableText = this.transform.parent.GetChild(6).GetComponent<Text>();

        if (this._buttons != null) {
            // http://answers.unity3d.com/questions/908847/passing-a-temporary-variable-to-add-listener.html
            // NB! Don't pass in variable i to delegate, needs a temporary value.
            for (int i = 0; i < this._buttons.Length; i++) {
                int tempValue = i;
                this._buttons[i].onClick.RemoveAllListeners();
                this._buttons[i].onClick.AddListener(() => this.onClick(tempValue));  // NB!
            }

            this.onClick(0);
        }

        if (this._nameInput != null) {
            this._nameInput.onValueChanged.RemoveAllListeners();
            this._nameInput.onValueChanged.AddListener((c) => this.onNameUpdate());

            this._nameInput.text = "";
        }
    }

    private void Start() {
        if (NetworkClient.allClients.Count > 0)
            NetworkClient.allClients[0].RegisterHandler((short)NetworkMessageType.MSG_NAME_AVAILABLE, this.recieveNetworkMessage);
    }

    // Updates the model selection index based on which button the player clicked on.
    private void onClick(int model) {
        if ((model < 0) || (model >= this._buttons.Length)) { return; }

        for (int i = 0; i < this._buttons.Length; i++)
            this._buttons[i].GetComponent<Image>().color = (i == model ? Color.yellow : Color.white);

        this.sendPlayerSelectMessage(model);
    }

    public void onNameUpdate() {
        this.sendPlayerNameMessage(this._nameInput.text.Trim());
    }

    // Recieve and handle the network message.
    private void recieveNetworkMessage(NetworkMessage message) {
        switch (message.msgType) {
            case (short)NetworkMessageType.MSG_NAME_AVAILABLE:
                this.updateNameAvailable(message.ReadMessage<IntegerMessage>());
                break;
            default:
                Debug.Log("ERROR! Unknown message type: " + message.msgType);
                break;
        }
    }

    // Update the name availability.
    private void updateNameAvailable(IntegerMessage message) {
        if (this._nameAvailableText == null)
            return;

        if (this._nameInput.text.Trim() == "") {
            this._nameAvailableText.text = "";
            return;
        }

        if (message.value != 0) {
            this._nameAvailableText.text = "name is available";
            this._nameAvailableText.color = new Color(0.0f, 0.4f, 0.0f);
        } else {
            this._nameAvailableText.text = "name is NOT available";
            this._nameAvailableText.color = Color.red;
        }
    }

    // Create the player select message, and send it to the server.
    private void sendPlayerSelectMessage(int model) {
        this.sendNetworkMessage(NetworkMessageType.MSG_PLAYER_SELECT, new IntegerMessage(model));
    }

    private void sendPlayerNameMessage(string name) {
        this.sendNetworkMessage(NetworkMessageType.MSG_PLAYER_NAME, new StringMessage(name));
    }

    // Send the network message to the server.
    private void sendNetworkMessage(NetworkMessageType messageType, MessageBase message) {
        if (NetworkClient.allClients.Count < 1)
            return;

        switch (messageType) {
            case NetworkMessageType.MSG_PLAYER_SELECT:
            case NetworkMessageType.MSG_PLAYER_NAME:
                NetworkClient.allClients[0].Send((short)messageType, message);
                break;
            default:
                Debug.Log("ERROR! Unknown message type: " + messageType);
                break;
        }
    }
}
