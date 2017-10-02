using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public enum NetworkMessageType {
    MSG_PLAYERSELECT = 1000, MSG_PLAYERCOUNT, MSG_PLAYERDIED, MSG_PLAYERWON, MSG_PLAYERKILLED
}

public class PlayerSelectMessage : MessageBase {
    public uint clientID;
    public int  selectedModel;
}

//
// https://docs.unity3d.com/ScriptReference/Networking.NetworkLobbyManager.html
//
public class NetworkPlayerSelect : NetworkLobbyManager {

    private string[]              _models     = { "PlayerCharacterBunny", "PlayerCharacterFox" };
    private int                   _players    = 0;
    private Dictionary<uint, int> _selections = new Dictionary<uint, int>();
    private Dictionary<int, bool> _isDead     = new Dictionary<int, bool>();
    private Dictionary<int, int>  _kills      = new Dictionary<int, int>();

    // Return the unique identifier for the lobby player object instance.
    private uint getClientID(NetworkConnection conn) {
        return (conn.playerControllers[0] != null ? conn.playerControllers[0].unetView.netId.Value : 0);
    }

    // Return the model selection made by the user.
    private int getSelectedModel(uint clientID) {
        return (this._selections.ContainsKey(clientID) ? this._selections[clientID] : 0);
    }

    // Register listening for player select messages from clients.
    public override void OnStartServer() {
        base.OnStartServer();

        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_PLAYERSELECT, this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_PLAYERCOUNT,  this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_PLAYERDIED,   this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_PLAYERKILLED, this.recieveNetworkMessage);

        this._players = 0;
        this._isDead.Clear();
        this._kills.Clear();
        this._selections.Clear();
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
        NetworkStartPosition[] spawnPoints    = Object.FindObjectsOfType<NetworkStartPosition>();
        Vector3                position       = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
        int                    selectedModel  = this.getSelectedModel(this.getClientID(conn));
        GameObject             playerPrefab   = Resources.Load<GameObject>("Prefabs/" + this._models[selectedModel]);
        GameObject             playerInstance = Instantiate(playerPrefab, position, playerPrefab.transform.rotation);

        this._isDead.Add(conn.connectionId, false);
        this._kills.Add(conn.connectionId,  0);
        this._players++;

        return playerInstance;
    }

    // Recieve and handle the network message.
    private void recieveNetworkMessage(NetworkMessage message) {
        switch (message.msgType) {
            case (short)NetworkMessageType.MSG_PLAYERSELECT:
                this.recievePlayerSelectMessage(message.ReadMessage<PlayerSelectMessage>());
                break;
            case (short)NetworkMessageType.MSG_PLAYERCOUNT:
                this.recievePlayerCountMessage(message.conn.connectionId);
                break;
            case (short)NetworkMessageType.MSG_PLAYERDIED:
                this.recievePlayerDiedMessage(message.conn.connectionId);
                break;
            case (short)NetworkMessageType.MSG_PLAYERKILLED:
                this.recievePlayerKilledMessage(message.ReadMessage<IntegerMessage>().value);
                break;
            default:
                Debug.Log("ERROR! Unknown message type: " + message.msgType);
                break;
        }
    }

    // Return the number of players still alive.
    private void recievePlayerCountMessage(int id) {
        NetworkServer.SendToClient(id, (short)NetworkMessageType.MSG_PLAYERCOUNT, new IntegerMessage(this._players));
    }

    // Update the clients when a player dies.
    private void recievePlayerDiedMessage(int id) {
        this._isDead[id] = true;

        // Tell the player who died what their ranking is.
        NetworkServer.SendToClient(id, (short)NetworkMessageType.MSG_PLAYERDIED, new IntegerMessage(this._players));

        // Decrease the number of players still alive.
        this._players--;

        // Tell all the players how many players are left.
        foreach (var connection in NetworkServer.connections) {
            int cID = connection.connectionId;

            this.recievePlayerCountMessage(cID);

            // If there is only one player left, tell them that they won.
            if ((this._players <= 1) && this._isDead.ContainsKey(cID) && !this._isDead[cID]) {
                NetworkServer.SendToClient(cID, (short)NetworkMessageType.MSG_PLAYERWON, new IntegerMessage(this._players));
            }
        }
    }

    // Increase the player kill count.
    private void recievePlayerKilledMessage(int connectionID) {
        //print("recievePlayerKilledMessage: " + connectionID);

        this._kills[connectionID]++;

        //print("recievePlayerKilledMessage::kills: " + this._kills[connectionID]);
    }

    // Parse the player select message, and select the player model.
    private void recievePlayerSelectMessage(PlayerSelectMessage message) {
        this.selectModel(message.clientID, message.selectedModel);
    }

    // Save the model selection made by the user.
    private void selectModel(uint clientID, int model) {
        if (!this._selections.ContainsKey(clientID))
            this._selections.Add(clientID, model);
        else
            this._selections[clientID] = model;
    }

    public virtual void OnServerDisconnect(NetworkConnection conn) {
        NetworkServer.DestroyPlayersForConnection(conn);
        if (conn.lastError != NetworkError.Ok) {
            if (LogFilter.logError) {
                Debug.LogError("ServerDisconnected due to error: " + conn.lastError);
            }
        }
    }

}
