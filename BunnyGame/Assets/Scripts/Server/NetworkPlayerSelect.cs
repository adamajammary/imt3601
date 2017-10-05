using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public enum NetworkMessageType {
    MSG_PLAYERSELECT = 1000, MSG_PLAYERCOUNT, MSG_PLAYERDIED, MSG_PLAYERWON, MSG_PLAYERKILL, MSG_PLAYERNAME
}

public class PlayerSelectMessage : MessageBase {
    public uint clientID;
    public int selectedModel;
}
public class PlayerNameMessage : MessageBase {
    public uint clientID;
    public string name;
}

//
// https://docs.unity3d.com/ScriptReference/Networking.NetworkLobbyManager.html
//
public class NetworkPlayerSelect : NetworkLobbyManager {

    private string[]                 _models     = { "PlayerCharacterBunny", "PlayerCharacterFox" };
    private int                      _players    = 0;
    private Dictionary<uint, int>    _selections = new Dictionary<uint, int>();
    private Dictionary<uint, string> _names      = new Dictionary<uint, string>();
    private Dictionary<int, bool>    _isDead     = new Dictionary<int, bool>();
    private Dictionary<int, int>     _kills      = new Dictionary<int, int>();

    // Return the unique identifier for the lobby player object instance.
    private uint getClientID(NetworkConnection conn) {
        return (conn.playerControllers[0] != null ? conn.playerControllers[0].unetView.netId.Value : 0);
    }

    // Return the model selection made by the user.
    private int getSelectedModel(uint clientID) {
        return (this._selections.ContainsKey(clientID) ? this._selections[clientID] : 0);
    }

    private string getPlayerName(uint clientID) {
        return (this._names.ContainsKey(clientID) ? this._names[clientID] : "NoName");
    }

    // Register listening for player select messages from clients.
    public override void OnStartServer() {
        base.OnStartServer();

        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_PLAYERSELECT,  this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_PLAYERCOUNT,   this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_PLAYERDIED,    this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_PLAYERNAME,    this.recieveNetworkMessage);

        this._players = 0;
        this._isDead.Clear();
        this._kills.Clear();
        this._selections.Clear();
        this._names.Clear();
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
        NetworkStartPosition[] spawnPoints      = FindObjectsOfType<NetworkStartPosition>();
        Vector3                position         = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
        int                    selectedModel    = this.getSelectedModel(this.getClientID(conn));
        GameObject             playerPrefab     = Resources.Load<GameObject>("Prefabs/" + this._models[selectedModel]);
        GameObject             playerInstance   = Instantiate(playerPrefab, position, playerPrefab.transform.rotation);
        BunnyController        bunnyScript      = playerInstance.GetComponent<BunnyController>();
        FoxController          foxScript        = playerInstance.GetComponent<FoxController>();
        PlayerInformation      playerInfo       = playerInstance.GetComponent<PlayerInformation>();

        playerInfo.ConnectionID = conn.connectionId;
        playerInfo.playerName = getPlayerName(this.getClientID(conn));
       

        this._isDead.Add(conn.connectionId, false);
        this._kills.Add(conn.connectionId,  0);
        this._players++;

        return playerInstance;
    }

    // This is called on the server when a client disconnects.
    public override void OnLobbyServerDisconnect(NetworkConnection conn) {
        this._isDead[conn.connectionId] = true;
        this._players--;

        // Tell all the other players how many players are left.
        foreach (var connection in NetworkServer.connections) {
            if ((connection == null) || (connection.connectionId == conn.connectionId))
                continue;

            int cID = connection.connectionId;

            this.sendPlayerCountMessage(cID);

            // If there is only one player left, tell them that they won.
            if ((this._players <= 1) && this._isDead.ContainsKey(cID) && !this._isDead[cID])
                this.sendPlayerWonMessage(cID);
        }

        if ((conn.lastError != NetworkError.Ok) && LogFilter.logError)
            Debug.LogError("ERROR! Client disconnected from server due to error: " + conn.lastError);

        NetworkServer.DestroyPlayersForConnection(conn);
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
                this.recievePlayerDiedMessage(message.conn.connectionId, message.ReadMessage<IntegerMessage>().value);
                break;
            case (short)NetworkMessageType.MSG_PLAYERNAME:
                this.recievePlayerNameMessage(message.ReadMessage<PlayerNameMessage>());
                break;
            default:
                Debug.Log("ERROR! Unknown message type: " + message.msgType);
                break;
        }
    }

    // Return the number of players still alive.
    private void recievePlayerCountMessage(int id) {
        this.sendPlayerCountMessage(id);
    }

    // Update the clients when a player dies.
    private void recievePlayerDiedMessage(int id, int killerID) {
        this._isDead[id] = true;

        if (killerID >= 0) {
            this._kills[killerID]++;
            this.sendPlayerKillMessage(killerID);
        }

        // Tell the player who died what their ranking is.
        this.sendPlayerDiedMessage(id);

        this._players--;

        // Tell all the players how many players are left.
        foreach (var connection in NetworkServer.connections) {
            int cID = connection.connectionId;

            this.sendPlayerCountMessage(cID);

            // If there is only one player left, tell them that they won.
            if ((this._players <= 1) && this._isDead.ContainsKey(cID) && !this._isDead[cID])
                this.sendPlayerWonMessage(cID);
        }
    }

    // Parse the player select message, and select the player model.
    private void recievePlayerSelectMessage(PlayerSelectMessage message)
    {
        this.selectModel(message.clientID, message.selectedModel);
    }
    // Parse the player name message, and set the player name.
    private void recievePlayerNameMessage(PlayerNameMessage message)
    {
        this.setName(message.clientID, message.name);
    }

    // Send the number of players still alive to the client.
    private void sendPlayerCountMessage(int id) {
        NetworkServer.SendToClient(id, (short)NetworkMessageType.MSG_PLAYERCOUNT, new IntegerMessage(this._players));
    }

    // Tell the player who died what their ranking is.
    private void sendPlayerDiedMessage(int id) {
        NetworkServer.SendToClient(id, (short)NetworkMessageType.MSG_PLAYERDIED, new IntegerMessage(this._players));
    }

    // Tell the player how many kills they have.
    private void sendPlayerKillMessage(int id) {
        NetworkServer.SendToClient(id, (short)NetworkMessageType.MSG_PLAYERKILL, new IntegerMessage(this._kills[id]));
    }

    // Tell the player that they won.
    private void sendPlayerWonMessage(int id) {
        NetworkServer.SendToClient(id, (short)NetworkMessageType.MSG_PLAYERWON, new IntegerMessage(this._players));
    }

    // Save the model selection made by the user.
    private void selectModel(uint clientID, int model) {
        if (!this._selections.ContainsKey(clientID))
            this._selections.Add(clientID, model);
        else
            this._selections[clientID] = model;
    }

    private void setName(uint clientID, string name) {
        if (!this._names.ContainsKey(clientID))
            this._names.Add(clientID, name);
        else
            this._names[clientID] = name;
    }
}
