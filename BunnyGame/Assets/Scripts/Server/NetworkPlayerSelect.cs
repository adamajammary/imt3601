using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public enum NetworkMessageType {
    MSG_PLAYER_SELECT = 1000,
    MSG_PLAYER_NAME,
    MSG_NAME_AVAILABLE,
    MSG_PLAYER_READY,
    MSG_KILLER_ID,
    MSG_PLAYER_STATS,
    MSG_GAME_OVER,
    MSG_LOBBY_UPDATE,
    MSG_LOBBY_PLAYERS,
    MSG_MATCH_DROP,
    MSG_MATCH_DISCONNECT,
    MSG_RANKINGS
}

public class Player {
    public int    id         = 0;
    public string name       = "";
    public int    model      = 0;
    public bool   isDead     = false;
    public int    placement  = 1;
    public int    rank       = 1;
    public int    kills      = 0;
    public int    score      = 0;
    public bool   win        = false;
    public bool   readyGame  = false;
    public bool   readyLobby = false;
}

public class LobbyPlayer {
    public string name   = "";
    public int    animal = 0;
    public bool   ready  = false;
}

public class GameOverMessage : MessageBase {
    public string   killer    = "";
    public string   name      = "";
    public int      model     = 0;
    public int      placement = 0;
    public int      kills     = 0;
    public bool     win       = false;
}

public class PlayerStatsMessage : MessageBase {
    public string name         = "";
    public int    playersAlive = 0;
    public int    kills        = 0;
    public string killer       = "";
    public string dead         = "";
}

public class LobbyPlayerMessage : MessageBase {
    public LobbyPlayer[] players;
}

public class RankingsMessage : MessageBase {
    public Player[] rankings;
}

//
// https://docs.unity3d.com/ScriptReference/Networking.NetworkLobbyManager.html
//
public class NetworkPlayerSelect : NetworkLobbyManager {

    private string[]                _models  = { "PlayerCharacterBunny", "PlayerCharacterFox", "PlayerCharacterBird" };
    private Dictionary<int, Player> _players = new Dictionary<int, Player>();
    
    private int getNrOfPlayersAlive() {
        int playersAlive = 0;

        foreach (var player in this._players) {
            if (!player.Value.isDead)
                playersAlive++;
        }

        return playersAlive;
    }

    // Returns the winner if there is one, otherwise it returns null.
    private Player getWinner() {
        if (this.getNrOfPlayersAlive() > 1)
            return null;

        foreach (var player in this._players) {
            if (!player.Value.isDead) {
                player.Value.placement = 1;
                player.Value.win       = true;

                return player.Value;
            }
        }

        return null;
    }

    // Checks if the specified name is available for the specified player ID.
    private bool isNameAvailable(int id, string name) {
        if (name == "")
            return true;

        foreach (var player in this._players) {
            if ((player.Key != id) && (player.Value.name == name))
                return false;
        }

        return true;
    }

    // This hook is invoked when a server is started - including when a host is started.
    public override void OnStartServer() {
        base.OnStartServer();
    }

    // This is called on the server when a new client connects.
    public override void OnServerConnect(NetworkConnection conn) {
        base.OnServerConnect(conn);

        if (this._players.ContainsKey(conn.connectionId))
            this._players[conn.connectionId].isDead = false;
    }

    // This is called on the server when a client disconnects.
    public override void OnServerDisconnect(NetworkConnection conn) {
        base.OnServerDisconnect(conn);

        if (this._players.ContainsKey(conn.connectionId)) {
            this._players[conn.connectionId].placement = Mathf.Max(1, this.getNrOfPlayersAlive());
            this._players[conn.connectionId].isDead    = true;
        }

        // Send updated lobby player info to all players.
        if (this.offlineScene == "Lobby")
            this.recieveLobbyUpdateMessage();
    }

    // Callback that happens when a NetworkMatch.DropConnection match request has been processed on the server.
    public override void OnDropConnection(bool success, string extendedInfo) {
        base.OnDropConnection(success, extendedInfo);
    }

    // Callback that happens when a NetworkMatch.DestroyMatch request has been processed on the server.
    public override void OnDestroyMatch(bool success, string extendedInfo) {
        base.OnDestroyMatch(success, extendedInfo);
    }

    // This is called on the server when the server is started - including when a host is started.
    public override void OnLobbyStartServer() {
        base.OnLobbyStartServer();

        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_PLAYER_SELECT, this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_PLAYER_NAME,   this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_PLAYER_READY,  this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_KILLER_ID,     this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_LOBBY_UPDATE,  this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_MATCH_DROP,    this.recieveNetworkMessage);

        this._players.Clear();
    }

    // This is called on the server when a new client connects to the server.
    public override void OnLobbyServerConnect(NetworkConnection conn) {
        base.OnLobbyServerConnect(conn);

        Player player = new Player();
        player.id     = conn.connectionId;
        player.name   = ("Player [#" + (conn.connectionId + 1) + "]");
        player.isDead = false;

        if (!this._players.ContainsKey(conn.connectionId))
            this._players.Add(conn.connectionId, player);
        else
            this._players[conn.connectionId] = player;
    }

    // This is called on the server when a client disconnects.
    // NB! This can happen in two situations: in the lobby and in-game.
    public override void OnLobbyServerDisconnect(NetworkConnection conn) {
        base.OnLobbyServerDisconnect(conn);

        if (this._players.ContainsKey(conn.connectionId)) {
            this._players[conn.connectionId].placement = Mathf.Max(1, this.getNrOfPlayersAlive());
            this._players[conn.connectionId].isDead    = true;
        }

        if (conn.lastError != NetworkError.Ok) {
            if ((conn.lastError != NetworkError.Timeout) && LogFilter.logError)
                Debug.LogError("ERROR! Client disconnected from server due to error: " + conn.lastError);
            return;
        }

        Player winner = this.getWinner();

        // If there is a winner, tell them that they won.
        if ((winner != null) && (winner.id != conn.connectionId))
            this.sendGameOverMessage(winner.id);

        // Send updated player/game stats to all players.
        foreach (var conn2 in NetworkServer.connections) {
            if ((conn2 == null) || !conn2.isReady || (conn2.connectionId == conn.connectionId) || !this._players.ContainsKey(conn2.connectionId))
                continue;

            this.sendPlayerStatsMessage(conn2.connectionId);

            if (winner != null)
                this.sendRankingsMessage(conn2.connectionId);
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
        matchMaker.SetMatchAttributes(matchInfo.networkId, false, 0, OnSetMatchAttributes);
        NetworkStartPosition[] spawnPoints = FindObjectsOfType<NetworkStartPosition>();

        if (spawnPoints.Length <= 0)
            return null;

        Vector3                position       = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
        int                    selectedModel  = this._players[conn.connectionId].model;
        GameObject             playerPrefab   = Resources.Load<GameObject>("Prefabs/Players/" + this._models[selectedModel]);
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
            case (short)NetworkMessageType.MSG_LOBBY_UPDATE:
                this.recieveLobbyUpdateMessage();
                break;
            case (short)NetworkMessageType.MSG_MATCH_DROP:
                this.recieveMatchDropMessage();
                break;
            default:
                Debug.Log("ERROR! Unknown message type: " + message.msgType);
                break;
        }
    }

    // Send updated lobby player info to all players.
    private void recieveLobbyUpdateMessage() {
        foreach (var conn in NetworkServer.connections) {
            if ((conn != null) && conn.isReady)
                this.sendLobbyPlayersMessage(conn.connectionId);
        }
    }

    // The match has dropped, tell the players to disconnect.
    private void recieveMatchDropMessage() {
        foreach (var conn in NetworkServer.connections) {
            if (conn != null)
                this.sendDisconnectMessage(conn.connectionId);
        }
    }

    // Send updated player stats to all players.
    private void recievePlayerReadyMessage(NetworkMessage message) {
        this._players[message.conn.connectionId].readyGame = true;

        foreach (var conn in NetworkServer.connections) {
            if ((conn != null) && (this._players[conn.connectionId].readyGame))
                this.sendPlayerStatsMessage(conn.connectionId);
        }
    }

    // Update the clients when a player dies.
    private void recievePlayerDiedMessage(NetworkMessage message) {
        int killerID     = message.ReadMessage<IntegerMessage>().value;
        int playersAlive = this.getNrOfPlayersAlive();

        // Tell the player who died what their ranking is.
        if (playersAlive > 1) {
            if (killerID >= 0)
                this._players[killerID].kills++;

            this._players[message.conn.connectionId].isDead    = true;
            this._players[message.conn.connectionId].placement = Mathf.Max(1, playersAlive);

            this.sendGameOverMessage(message.conn.connectionId, killerID, false);
        }

        Player winner = this.getWinner();

        // If there is a winner, tell them that they won.
        if (winner != null)
            this.sendGameOverMessage(winner.id);

        // Send updated player/game stats to all players.
        foreach (var conn in NetworkServer.connections) {
            if ((conn == null) || !conn.isReady || !this._players.ContainsKey(conn.connectionId))
                continue;

            this.sendPlayerStatsMessage(conn.connectionId, killerID, message.conn.connectionId);

            if (winner != null)
                this.sendRankingsMessage(conn.connectionId);
        }
    }

    // Parse the player select message, and select the player model.
    private void recievePlayerSelectMessage(NetworkMessage message) {
        this._players[message.conn.connectionId].model = message.ReadMessage<IntegerMessage>().value;

        foreach (var conn in NetworkServer.connections) {
            if (conn != null)
                this.sendLobbyPlayersMessage(conn.connectionId);
        }
    }

    // Parse the player name message, and update the player name.
    private void recievePlayerNameMessage(NetworkMessage message) {
        this.sendNameAvailableMessage(message.conn.connectionId, message.ReadMessage<StringMessage>().value);

        foreach (var conn in NetworkServer.connections) {
            if (conn != null)
                this.sendLobbyPlayersMessage(conn.connectionId);
        }
    }

    // Tell the client to disconnect from the match.
    private void sendDisconnectMessage(int id) {
        NetworkServer.SendToClient(id, (short)NetworkMessageType.MSG_MATCH_DISCONNECT, new IntegerMessage());
    }

    // Tell the player wether the name they chose is available or not.
    private void sendNameAvailableMessage(int id, string name) {
        bool available = this.isNameAvailable(id, name);

        if (name == "")
            name = ("Player [#" + (id + 1) + "]");

        if (available)
            this._players[id].name = name;

        NetworkServer.SendToClient(id, (short)NetworkMessageType.MSG_NAME_AVAILABLE, new IntegerMessage(available ? 1 : 0));
    }

    // Tell the player the end game stats.
    private void sendGameOverMessage(int id, int killerID = -1, bool win = true) {
        GameOverMessage message = new GameOverMessage();

        message.killer    = (killerID >= 0 ? this._players[killerID].name : "");
        message.name      = this._players[id].name;
        message.model     = this._players[id].model;
        message.placement = this._players[id].placement;
        message.kills     = this._players[id].kills;
        message.win       = this._players[id].win;

        NetworkServer.SendToClient(id, (short)NetworkMessageType.MSG_GAME_OVER, message);
    }

    // Send a list of all the player names and ready-state to the player.
    private void sendLobbyPlayersMessage(int id) {
        LobbyPlayerMessage message = new LobbyPlayerMessage();
        List<Player>       players = new List<Player>();

        // Update the ready-state for each connected lobby player.
        foreach (var conn in NetworkServer.connections) {
            NetworkLobbyPlayer lobbyPlayer = null;

            if ((conn != null) && (conn.playerControllers.Count > 0) && (conn.playerControllers[0].gameObject != null))
                lobbyPlayer = conn.playerControllers[0].gameObject.GetComponent<NetworkLobbyPlayer>();

            if (lobbyPlayer != null) {
                this._players[conn.connectionId].readyLobby = lobbyPlayer.readyToBegin;
                players.Add(this._players[conn.connectionId]);
            }
        }

        message.players = new LobbyPlayer[players.Count];

        for (int i = 0; i < players.Count; i++) {
            message.players[i]        = new LobbyPlayer();
            message.players[i].name   = players[i].name;
            message.players[i].animal = players[i].model;
            message.players[i].ready  = players[i].readyLobby;
        }

        NetworkServer.SendToClient(id, (short)NetworkMessageType.MSG_LOBBY_PLAYERS, message);
    }

    // Tell the player how many players are still alive.
    private void sendPlayerStatsMessage(int id, int killerID = -1, int deadID = -1) {
        PlayerStatsMessage message = new PlayerStatsMessage();

        message.name         = this._players[id].name;
        message.playersAlive = this.getNrOfPlayersAlive();
        message.kills        = this._players[id].kills;
        message.killer       = (killerID >= 0 ? this._players[killerID].name : "");
        message.dead         = (deadID   >= 0 ? this._players[deadID].name   : "");

        NetworkServer.SendToClient(id, (short)NetworkMessageType.MSG_PLAYER_STATS, message);
    }

    // Send a list of the end-game rankings to the player.
    private void sendRankingsMessage(int id) {
        RankingsMessage message = new RankingsMessage();

        // Sort/rank players by score.
        List<Player> rankings = new List<Player>();

        foreach (var player in this._players) {
            player.Value.score  = 0;
            player.Value.score += ((7 - player.Value.placement) * 1000);
            player.Value.score += (player.Value.kills * 600);

            rankings.Add(player.Value);
        }

        rankings.Sort((p1, p2) => p2.score.CompareTo(p1.score));

        // NB! Since we can't send dictionaries as network messages, we must send it as an array.
        message.rankings = new Player[rankings.Count];

        for (int i = 0; i < rankings.Count; i++) {
            message.rankings[i]        = new Player();
            message.rankings[i].id     = rankings[i].id;
            message.rankings[i].name   = rankings[i].name;
            message.rankings[i].model  = rankings[i].model;
            message.rankings[i].isDead = rankings[i].isDead;
            message.rankings[i].rank   = (i + 1);
            message.rankings[i].kills  = rankings[i].kills;
            message.rankings[i].score  = rankings[i].score;
            message.rankings[i].win    = rankings[i].win;

            // Only send the scores once form the server.
            if (id == NetworkServer.serverHostId)
                Leaderboard.SaveScore(rankings[i].name, (double)rankings[i].score);
        }

        NetworkServer.SendToClient(id, (short)NetworkMessageType.MSG_RANKINGS, message);
    }
}
