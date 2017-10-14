using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public enum NetworkMessageType {
    MSG_PLAYER_SELECT = 1000, MSG_PLAYER_NAME, MSG_NAME_AVAILABLE, MSG_PLAYER_READY, MSG_KILLER_ID, MSG_PLAYER_STATS, MSG_GAME_OVER
}

public class Player {
    public int    id     = 0;
    public string name   = "";
    public int    model  = 0;
    public bool   isDead = false;
    public int    kills  = 0;
    public int    rank   = 0;
    public bool   win    = false;
}

public class GameOverMessage : MessageBase {
    public string    killer   = "";
    public string    name     = "";
    public int       rank     = 0;
    public int       kills    = 0;
    public bool      win      = false;
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

    private string[]                _models       = { "PlayerCharacterBunny", "PlayerCharacterFox" };
    private int                     _playersAlive = 0;
    private Dictionary<int, Player> _players      = new Dictionary<int, Player>();

    // Returns the winner if there is one, otherwise it returns null.
    private Player getWinner() {
        if (this._playersAlive > 1)
            return null;

        foreach (var player in this._players) {
            if (!player.Value.isDead) {
                player.Value.rank = 1;
                player.Value.win  = true;

                return player.Value;
            }
        }

        return null;
    }

    // Checks if the specified name is available for the specified player ID.
    private bool isNameAvailable(int id, string name) {
        foreach (var player in this._players) {
            if ((player.Key != id) && (player.Value.name == name))
                return false;
        }

        return true;
    }

    // This is called on the server when the server is started - including when a host is started.
    public override void OnLobbyStartServer() {
        base.OnLobbyStartServer();

        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_PLAYER_SELECT, this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_PLAYER_NAME,   this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_PLAYER_READY,  this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_KILLER_ID,     this.recieveNetworkMessage);

        this._players.Clear();
    }

    // This is called on the server when a new client connects to the server.
    public override void OnLobbyServerConnect(NetworkConnection conn) {
        base.OnLobbyServerConnect(conn);

        Player player = new Player();

        player.id   = conn.connectionId;
        player.name = ("Player [#" + (conn.connectionId + 1) + "]");

        this._players.Add(conn.connectionId, player);
        this._playersAlive++;
    }

    // This is called on the server when a client disconnects.
    // NB! This can happen in two situations: in the lobby and in-game.
    public override void OnLobbyServerDisconnect(NetworkConnection conn) {
        base.OnLobbyServerDisconnect(conn);
        //NetworkServer.DestroyPlayersForConnection(conn);
        //conn.Disconnect();

        this._players.Remove(conn.connectionId);
        this._playersAlive--;

        if (conn.lastError != NetworkError.Ok) {
            if ((conn.lastError != NetworkError.Timeout) && LogFilter.logError)
                Debug.LogError("ERROR! Client disconnected from server due to error: " + conn.lastError);
            return;
        }

        if (this.offlineScene == "Lobby")
            return;

        Player winner = this.getWinner();

        // Send updated stats to the remaining players in-game.
        foreach (var conn2 in NetworkServer.connections) {
            if ((conn2 != null) && conn2.isReady && (conn2.connectionId != conn.connectionId) && this._players.ContainsKey(conn2.connectionId))
                this.sendPlayerStatsMessage(conn2.connectionId);
        }

        // If there is a winner, tell them that they won.
        if (winner != null) {
            this.sendGameOverMessage(winner.id);
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
        int                    selectedModel  = this._players[conn.connectionId].model;
        GameObject             playerPrefab   = Resources.Load<GameObject>("Prefabs/" + this._models[selectedModel]);
        GameObject             playerInstance = Instantiate(playerPrefab, position, playerPrefab.transform.rotation);
        PlayerInformation      playerInfo     = playerInstance.GetComponent<PlayerInformation>();

        playerInfo.ConnectionID = conn.connectionId;
        playerInfo.playerName   = this._players[conn.connectionId].name;

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
            this.sendPlayerStatsMessage(conn.connectionId);
        }
    }

    // Update the clients when a player dies.
    private void recievePlayerDiedMessage(NetworkMessage message) {
        int killerID = message.ReadMessage<IntegerMessage>().value;

        // Tell the player who died what their ranking is.
        if (this._playersAlive > 1) {
            if (killerID >= 0)
                this._players[killerID].kills++;

            this._players[message.conn.connectionId].isDead = true;
            this._players[message.conn.connectionId].rank   = this._playersAlive;

            this.sendGameOverMessage(message.conn.connectionId, killerID, false);
            this._playersAlive--;
        }

        // Tell all the players how many players are left.
        foreach (var conn in NetworkServer.connections) {
            if ((conn != null) && conn.isReady || this._players.ContainsKey(conn.connectionId))
                this.sendPlayerStatsMessage(conn.connectionId, killerID, message.conn.connectionId);
        }

        Player winner = this.getWinner();

        // If there is a winner, tell them that they won.
        if (winner != null) {
            this.sendGameOverMessage(winner.id);
        }
    }

    // Parse the player select message, and select the player model.
    private void recievePlayerSelectMessage(NetworkMessage message) {
        this._players[message.conn.connectionId].model = message.ReadMessage<IntegerMessage>().value;
    }

    // Parse the player name message, and set the player name.
    private void recievePlayerNameMessage(NetworkMessage message) {
        this.sendNameAvailableMessage(message.conn.connectionId, message.ReadMessage<StringMessage>().value);
    }

    // Tells the player wether the name they chose is available or not.
    private void sendNameAvailableMessage(int id, string name) {
        if (name == "") {
            name = ("Player [#" + (id + 1) + "]");
        } else {
            bool available = this.isNameAvailable(id, name);

            if (available)
                this._players[id].name = name;
            
            NetworkServer.SendToClient(id, (short)NetworkMessageType.MSG_NAME_AVAILABLE, new IntegerMessage(available ? 1 : 0));
        }
    }

    // Tells the player the end game stats.
    private void sendGameOverMessage(int id, int killerID = -1, bool win = true) {
        GameOverMessage message = new GameOverMessage();

        message.killer = (killerID >= 0 ? this._players[killerID].name : "");
        message.name   = this._players[id].name;
        message.rank   = this._players[id].rank;
        message.kills  = this._players[id].kills;
        message.win    = this._players[id].win;

        NetworkServer.SendToClient(id, (short)NetworkMessageType.MSG_GAME_OVER, message);
    }

    // Tells the player how many players are still alive.
    private void sendPlayerStatsMessage(int id, int killerID = -1, int deadID = -1) {
        PlayerStatsMessage message = new PlayerStatsMessage();

        message.name         = this._players[id].name;
        message.playersAlive = this._playersAlive;
        message.kills        = this._players[id].kills;
        message.killer       = (killerID >= 0 ? this._players[killerID].name : "");
        message.dead         = (deadID   >= 0 ? this._players[deadID].name   : "");

        NetworkServer.SendToClient(id, (short)NetworkMessageType.MSG_PLAYER_STATS, message);
    }

    public string getName(int id)
    {
        if (this._players.ContainsKey(id))
            return this._players[id].name;
        else
            return "";
    }

    public int getSelection(int id) {
        if (this._players.ContainsKey(id))
            return this._players[id].model;
        else
            return -1;
    }
}
