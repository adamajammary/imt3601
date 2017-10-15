using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;

public class PlayerSelectCanvasEvents : NetworkBehaviour {

    public Button[]   _buttons;
    public InputField _nameInput;

    // This function is called when the object becomes enabled and active.
    // ISSUE #67: When the player re-connects in the lobby manager, the GUI is not refreshed if it re-uses the last connection.
    // SOLUTION:  Reset input text and button selections whenever the canvas object is re-enabled (re-connected).
    private void OnEnable() {
        this._buttons   = this.transform.parent.GetChild(3).GetComponentsInChildren<Button>();
        this._nameInput = this.transform.parent.GetChild(2).GetComponent<InputField>();

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

        if (this._nameInput != null)
            this._nameInput.text = "";
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
        this.SendNetworkMessage(NetworkMessageType.MSG_PLAYER_SELECT, new IntegerMessage(model));
    }

    private void SendPlayerNameMessage(string name) {
        this.SendNetworkMessage(NetworkMessageType.MSG_PLAYER_NAME, new StringMessage(name));
    }

    // Send the network message to the server.
    private void SendNetworkMessage(NetworkMessageType messageType, MessageBase message) {
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
