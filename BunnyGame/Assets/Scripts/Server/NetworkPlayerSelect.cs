using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public enum NetworkMessageType {
    MSG_PLAYER_SELECT = 1000, MSG_PLAYER_NAME, MSG_NAME_AVAILABLE, MSG_PLAYER_READY, MSG_KILLER_ID, MSG_PLAYER_STATS, MSG_GAME_OVER
}

public class GameOverMessage : MessageBase {
    public string killer = "";
    public string name   = "";
    public int    rank   = 0;
    public int    kills  = 0;
    public bool   win    = false;
}

public class PlayerStatsMessage : MessageBase {
    public string name         = "";
    public int    playersAlive = 0;
    public int    kills        = 0;
    public string killer       = "";
    public string dead         = "";
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

    // This is called on the server when the server is started - including when a host is started.
    public override void OnLobbyStartServer() {
        base.OnLobbyStartServer();

        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_PLAYER_SELECT, this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_PLAYER_NAME,   this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_PLAYER_READY,  this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_KILLER_ID,     this.recieveNetworkMessage);

        this._players = 0;
        this._isDead.Clear();
        this._kills.Clear();
        this._selections.Clear();
        this._names.Clear();
    }

    // This is called on the server when a new client connects to the server.
    public override void OnLobbyServerConnect(NetworkConnection conn) {
        base.OnLobbyServerConnect(conn);

        this._isDead.Add(conn.connectionId,     false);
        this._kills.Add(conn.connectionId,      0);
        this._selections.Add(conn.connectionId, 0);
        this._names.Add(conn.connectionId,      ("Player [#" + (conn.connectionId + 1) + "]"));

        this._players++;
    }

    // This is called on the server when a client disconnects.
    // NB! This can happen in two situations: in the lobby and in-game.
    public override void OnLobbyServerDisconnect(NetworkConnection conn) {
        base.OnLobbyServerDisconnect(conn);
        //NetworkServer.DestroyPlayersForConnection(conn);
        //conn.Disconnect();

        this._isDead.Remove(conn.connectionId);
        this._kills.Remove(conn.connectionId);
        this._selections.Remove(conn.connectionId);
        this._names.Remove(conn.connectionId);

        this._players--;

        if (conn.lastError != NetworkError.Ok) {
            if ((conn.lastError != NetworkError.Timeout) && LogFilter.logError)
                Debug.LogError("ERROR! Client disconnected from server due to error: " + conn.lastError);
            return;
        }

        if (this.offlineScene == "Lobby")
            return;

        // Send updated stats to the remaining players in-game.
        foreach (var conn2 in NetworkServer.connections) {
            if ((conn2 == null) || !conn2.isReady || (conn2.connectionId == conn.connectionId) || !this._isDead.ContainsKey(conn2.connectionId))
                continue;

            this.sendPlayerStatsMessage(conn2.connectionId);

            // If there is only one player left, tell them that they won.
            if ((this._players <= 1) && !this._isDead[conn2.connectionId])
                this.sendGameOverMessage(conn2.connectionId);
        }
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
        int                    selectedModel  = this._selections[conn.connectionId];
        GameObject             playerPrefab   = Resources.Load<GameObject>("Prefabs/" + this._models[selectedModel]);
        GameObject             playerInstance = Instantiate(playerPrefab, position, playerPrefab.transform.rotation);
        PlayerInformation      playerInfo     = playerInstance.GetComponent<PlayerInformation>();

        playerInfo.ConnectionID = conn.connectionId;
        playerInfo.playerName   = this._names[conn.connectionId];

        return playerInstance;
    }

    // Recieve and handle the network message.
    private void recieveNetworkMessage(NetworkMessage message) {
        switch (message.msgType) {
            case (short)NetworkMessageType.MSG_PLAYER_SELECT:
                this.recievePlayerSelectMessage(message);
                break;
            case (short)NetworkMessageType.MSG_PLAYER_NAME:
                this.recievePlayerNameMessage(message);
                break;
            case (short)NetworkMessageType.MSG_PLAYER_READY:
                this.recievePlayerReadyMessage(message);
                break;
            case (short)NetworkMessageType.MSG_KILLER_ID:
                this.recievePlayerDiedMessage(message);
                break;
            default:
                Debug.Log("ERROR! Unknown message type: " + message.msgType);
                break;
        }
    }

    // Send updated player stats to all players.
    private void recievePlayerReadyMessage(NetworkMessage message) {
        foreach (var conn in NetworkServer.connections) {
            if ((conn != null) && conn.isReady) {
                this.sendPlayerStatsMessage(conn.connectionId);
            }
        }
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
        foreach (var conn in NetworkServer.connections) {
            if ((conn == null) || !conn.isReady || !this._isDead.ContainsKey(conn.connectionId))
                continue;

            this.sendPlayerStatsMessage(conn.connectionId, killerID, message.conn.connectionId);

            // If there is only one player left, tell them that they won.
            if ((this._players <= 1) && !this._isDead[conn.connectionId])
                this.sendGameOverMessage(conn.connectionId);
        }
    }

    // Parse the player select message, and select the player model.
    private void recievePlayerSelectMessage(NetworkMessage message) {
        this.selectModel(message.conn.connectionId, message.ReadMessage<IntegerMessage>().value);
    }

    // Parse the player name message, and set the player name.
    private void recievePlayerNameMessage(NetworkMessage message) {
        this.sendNameAvailableMessage(message.conn.connectionId, message.ReadMessage<StringMessage>().value);
    }

    // Tells the player wether the name they chose is available or not.
    private void sendNameAvailableMessage(int id, string name) {
        bool isNameAvailable = true;

        foreach (var n in this._names) {
            if ((n.Key != id) && (n.Value == name)) {
                isNameAvailable = false;
                break;
            }
        }

        if (isNameAvailable || (name == ""))
            this.setName(id, name);

        NetworkServer.SendToClient(id, (short)NetworkMessageType.MSG_NAME_AVAILABLE, new IntegerMessage(isNameAvailable ? 1 : 0));
    }

    // Tells the player the end game stats.
    private void sendGameOverMessage(int id, int killerID = -1, bool win = true) {
        GameOverMessage message = new GameOverMessage();

        message.killer = (killerID < 0 ? "" : this._names[killerID]);
        message.name   = this._names[id];
        message.rank   = (win ? 1 : this._players);
        message.kills  = this._kills[id];
        message.win    = win;

        NetworkServer.SendToClient(id, (short)NetworkMessageType.MSG_GAME_OVER, message);
    }

    // Tells the player how many players are still alive.
    private void sendPlayerStatsMessage(int id, int killerID = -1, int deadID = -1) {
        PlayerStatsMessage message = new PlayerStatsMessage();

        message.name         = this._names[id];
        message.playersAlive = this._players;
        message.kills        = this._kills[id];
        message.killer       = (killerID < 0 ? "" : this._names[killerID]);
        message.dead         = (deadID   < 0 ? "" : this._names[deadID]);

        NetworkServer.SendToClient(id, (short)NetworkMessageType.MSG_PLAYER_STATS, message);
    }

    // Save the model selection made by the user.
    private void selectModel(int id, int model) {
        if (model < 0)
            return;

        if (!this._selections.ContainsKey(id))
            this._selections.Add(id, model);
        else
            this._selections[id] = model;
    }

    private void setName(int id, string name) {
        if (name == "")
            name = ("Player [#" + (id + 1) + "]");

        if (!this._names.ContainsKey(id))
            this._names.Add(id, name);
        else
            this._names[id] = name;
    }
}
