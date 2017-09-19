using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MetworkMessageType {
    public const short MSG_PLAYERSELECT = 1000;
};

public class PlayerSelectMessage : MessageBase {
    public int connectionId;
    public int selectedModel;
}

//
// https://docs.unity3d.com/ScriptReference/Networking.NetworkLobbyManager.html
//
public class NetworkPlayerSelect : NetworkLobbyManager {

    private string[]             _models     = { "PlayerCharacterBunny", "PlayerCharacterFox" };
    private Dictionary<int, int> _selections = new Dictionary<int, int>();

    // Register listening for player select messages from clients.
    public override void OnStartServer() {
        base.OnStartServer();
        NetworkServer.RegisterHandler(MetworkMessageType.MSG_PLAYERSELECT, this.RecieveNetworkMessage);
    }

    //
    // This allows customization of the creation of the GamePlayer object on the server.
    // NB! This event happens after creating the lobby player (OnLobbyServerCreateLobbyPlayer - still in the lobby scene)
    // NB! When we enter this event we are in the game scene and have no access to lobby scene objects that have been destroyed.
    //
    // Load the player character with the selected animal model and all required components.
    // NB! Prefabs for this has to be stored in "Assets/Resources/Prefabs/".
    //
    public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId) {
        int        selectedModel  = this.GetSelectedModel(conn.connectionId);
        GameObject playerPrefab   = Resources.Load<GameObject>("Prefabs/" + this._models[selectedModel]);
        GameObject playerInstance = Instantiate(playerPrefab, new Vector3(Random.Range(-40, 40), 10, Random.Range(-40, 40)), playerPrefab.transform.rotation);

        foreach (Transform model in playerInstance.transform) {
            model.gameObject.tag     = playerInstance.tag;
            model.transform.rotation = playerInstance.transform.rotation;

            foreach (Transform mesh in model.transform) {
                mesh.gameObject.tag = model.gameObject.tag;
            }
        }

        return playerInstance;
    }

    // Parse the network message, and forward handling to the specific method.
    private void RecieveNetworkMessage(NetworkMessage message) {
        switch (message.msgType) {
            case MetworkMessageType.MSG_PLAYERSELECT:
                this.RecievePlayerSelectMessage(message.ReadMessage<PlayerSelectMessage>());
                break;
            default:
                Debug.Log("ERROR! Unknown message type: " + message.msgType);
                break;
        }
    }

    // Parse the player select message, and select the player model.
    private void RecievePlayerSelectMessage(PlayerSelectMessage message) {
        this.SelectModel(message.connectionId, message.selectedModel);
    }

    // Save the model selection made by the user.
    public void SelectModel(int id, int model) {
        if (!this._selections.ContainsKey(id))
            this._selections.Add(id, model);
        else
            this._selections[id] = model;
    }

    // Return the model selection made by the user.
    public int GetSelectedModel(int id) {
        if (!this._selections.ContainsKey(id)) {
            Debug.Log("ERROR! Unknown model type selected: " + id);
            return -1;
        }

        return this._selections[id];
    }
}
