using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public enum NetworkMessageType {
    MSG_PLAYERSELECT = 1000, MSG_PLAYERNAME, MSG_PLAYERCOUNT, MSG_GAME_OVER, MSG_KILLER_ID
}

public class GameOverMessage : MessageBase {
    public string killer = "";
    public string name   = "";
    public int    rank   = 0;
    public int    kills  = 0;
    public bool   win    = false;
}

//
// https://docs.unity3d.com/ScriptReference/Networking.NetworkLobbyManager.html
//
public class NetworkPlayerSelect : NetworkLobbyManager {

    private string[]                _models     = { "PlayerCharacterBunny", "PlayerCharacterFox" };
    private int                     _players    = 0;
    private Dictionary<int, int>    _selections = new Dictionary<int, int>();
    private Dictionary<int, string> _names      = new Dictionary<int, string>();
    private Dictionary<int, bool>   _isDead     = new Dictionary<int, bool>();
    private Dictionary<int, int>    _kills      = new Dictionary<int, int>();

    // Return the model selection made by the user.
    private int getSelectedModel(int id) {
        return (this._selections.ContainsKey(id) ? this._selections[id] : 0);
    }

    private string getPlayerName(int id) {
        return (this._names.ContainsKey(id) ? this._names[id] : "NoName");
    }

    // Register listening for player select messages from clients.
    public override void OnStartServer() {
        base.OnStartServer();

        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_PLAYERSELECT, this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_PLAYERNAME,   this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_PLAYERCOUNT,  this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_GAME_OVER,    this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_KILLER_ID,    this.recieveNetworkMessage);

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
        NetworkStartPosition[] spawnPoints    = FindObjectsOfType<NetworkStartPosition>();
        Vector3                position       = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
        int                    selectedModel  = this.getSelectedModel(conn.connectionId);
        GameObject             playerPrefab   = Resources.Load<GameObject>("Prefabs/" + this._models[selectedModel]);
        GameObject             playerInstance = Instantiate(playerPrefab, position, playerPrefab.transform.rotation);
        PlayerInformation      playerInfo     = playerInstance.GetComponent<PlayerInformation>();

        playerInfo.ConnectionID = conn.connectionId;
        playerInfo.playerName   = this.getPlayerName(conn.connectionId);

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
                this.sendGameOverMessage(cID);
        }

        if ((conn.lastError != NetworkError.Ok) && LogFilter.logError)
            Debug.LogError("ERROR! Client disconnected from server due to error: " + conn.lastError);

        NetworkServer.DestroyPlayersForConnection(conn);
    }

    // Recieve and handle the network message.
    private void recieveNetworkMessage(NetworkMessage message) {
        switch (message.msgType) {
            case (short)NetworkMessageType.MSG_PLAYERSELECT:
                this.recievePlayerSelectMessage(message);
                break;
            case (short)NetworkMessageType.MSG_PLAYERCOUNT:
                this.recievePlayerCountMessage(message);
                break;
            case (short)NetworkMessageType.MSG_KILLER_ID:
                this.recievePlayerDiedMessage(message);
                break;
            case (short)NetworkMessageType.MSG_PLAYERNAME:
                this.recievePlayerNameMessage(message);
                break;
            default:
                Debug.Log("ERROR! Unknown message type: " + message.msgType);
                break;
        }
    }

    // Return the number of players still alive.
    private void recievePlayerCountMessage(NetworkMessage message) {
        this.sendPlayerCountMessage(message.conn.connectionId);
    }

    // Update the clients when a player dies.
    private void recievePlayerDiedMessage(NetworkMessage message) {
        int killerID = message.ReadMessage<IntegerMessage>().value;

        this._isDead[message.conn.connectionId] = true;

        if (killerID >= 0)
            this._kills[killerID]++;

        // Tell the player who died what their ranking is.
        this.sendGameOverMessage(message.conn.connectionId, killerID, false);

        this._players--;

        // Tell all the players how many players are left.
        foreach (var connection in NetworkServer.connections) {
            int cID = connection.connectionId;

            this.sendPlayerCountMessage(cID);

            // If there is only one player left, tell them that they won.
            if ((this._players <= 1) && this._isDead.ContainsKey(cID) && !this._isDead[cID])
                this.sendGameOverMessage(cID);
        }
    }

    // Parse the player select message, and select the player model.
    private void recievePlayerSelectMessage(NetworkMessage message) {
        this.selectModel(message.conn.connectionId, message.ReadMessage<IntegerMessage>().value);
    }

    // Parse the player name message, and set the player name.
    private void recievePlayerNameMessage(NetworkMessage message) {
        this.setName(message.conn.connectionId, message.ReadMessage<StringMessage>().value);
    }

    // Send the number of players still alive to the client.
    private void sendPlayerCountMessage(int id) {
        NetworkServer.SendToClient(id, (short)NetworkMessageType.MSG_PLAYERCOUNT, new IntegerMessage(this._players));
    }

    // Tell the player the end game stats.
    private void sendGameOverMessage(int id, int killerID = -1, bool win = true) {
        GameOverMessage message = new GameOverMessage();

        message.killer = (killerID < 0 ? "" : this.getPlayerName(killerID));
        message.name   = this.getPlayerName(id);
        message.rank   = (win ? 1 : this._players);
        message.kills  = this._kills[id];
        message.win    = win;

        print("message.killer: " + message.killer);

        NetworkServer.SendToClient(id, (short)NetworkMessageType.MSG_GAME_OVER, message);
    }

    // Save the model selection made by the user.
    private void selectModel(int id, int model) {
        if (!this._selections.ContainsKey(id))
            this._selections.Add(id, model);
        else
            this._selections[id] = model;
    }

    private void setName(int id, string name) {
        if (!this._names.ContainsKey(id))
            this._names.Add(id, name);
        else
            this._names[id] = name;
    }
}
