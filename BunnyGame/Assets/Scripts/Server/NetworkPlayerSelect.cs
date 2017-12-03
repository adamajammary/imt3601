using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public enum KillerID {
    KILLER_ID_FALL = -1000,
    KILLER_ID_WALL,
    KILLER_ID_WATER
}

public enum NetworkMessageType {
    MSG_PLAYER_SELECT = 1000,
    MSG_PLAYER_NAME,
    MSG_NAME_AVAILABLE,
    MSG_PLAYER_READY,
    MSG_KILLER_ID,
    MSG_ATTACK,
    MSG_PLAYER_STATS,
    MSG_GAME_OVER,
    MSG_LOBBY_UPDATE,
    MSG_LOBBY_PLAYERS,
    MSG_MATCH_DROP,
    MSG_MATCH_DISCONNECT,
    MSG_RANKINGS,
    MSG_MAP_SELECT,
    MSG_GAMEMODE_SELECT,
    MSG_MAP_VOTE,
    MSG_GAMEMODE_VOTE,
    MSG_DATA_FILE_LOADING,
    MSG_DATA_FILE_PROGRESS,
    MSG_DATA_FILE_READY
}

public class Player {
    public int    id         = 0;
    public string name       = "";
    public int    animal     = 0;
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

public class LoadingPlayer {
    public bool  loading  = true;
    public int   progress = 0;
}

public class GameOverMessage : MessageBase {
    public string   killer    = "";
    public string   name      = "";
    public int      animal    = 0;
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

public class AttackMessage : MessageBase {
    public float      damageAmount   = 0.0f;
    public int        attackerID     = 0;
    public int        victimID       = 0;
    public Vector3    impactPosition = new Vector3();
}

//
// https://docs.unity3d.com/ScriptReference/Networking.NetworkLobbyManager.html
//
public class NetworkPlayerSelect : NetworkLobbyManager {

    private string[]                              _islands          = { "Island", "Island42" };
    private string[]                              _gamemodes        = { "Battleroyale", "Deathmatch" };
    private string[]                              _models           = { "PlayerCharacterBunny", "PlayerCharacterFox", "PlayerCharacterBird", "PlayerCharacterMoose" };
    private Dictionary<int, Player>               _players          = new Dictionary<int, Player>();
    private Dictionary<NetworkConnection, string> _mapVotes         = new Dictionary<NetworkConnection, string>();
    private Dictionary<NetworkConnection, string> _gamemodeVotes    = new Dictionary<NetworkConnection, string>();
    private Dictionary<int, LoadingPlayer>        _isLoading        = new Dictionary<int, LoadingPlayer>();

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

    // Checks if all clients have completed loading their data files.
    public bool IsDataLoadingComplete() {
        // Assuming no client starts loading after another has completed loading.
        foreach (var client in this._isLoading) {
            if (client.Value.loading)
                return false;
        }

        return true;
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

        this.resetMapSelect(conn);
    }

    // This is called on the server when a client disconnects.
    public override void OnServerDisconnect(NetworkConnection conn) {
        base.OnServerDisconnect(conn);

        if (this._players.ContainsKey(conn.connectionId)) {
            this._players[conn.connectionId].placement = Mathf.Max(1, this.getNrOfPlayersAlive());
            this._players[conn.connectionId].isDead    = true;
        }

        this.resetMapSelect(conn);
        this.resetGameModeSelect(conn);

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

        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_PLAYER_SELECT,      this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_PLAYER_NAME,        this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_PLAYER_READY,       this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_KILLER_ID,          this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_ATTACK,             this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_LOBBY_UPDATE,       this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_MATCH_DROP,         this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_MAP_SELECT,         this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_GAMEMODE_SELECT,    this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_DATA_FILE_LOADING,  this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_DATA_FILE_PROGRESS, this.recieveNetworkMessage);
        NetworkServer.RegisterHandler((short)NetworkMessageType.MSG_DATA_FILE_READY,    this.recieveNetworkMessage);

        this._players.Clear();
        this._mapVotes.Clear();
        this._gamemodeVotes.Clear();
        this._isLoading.Clear();
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

        this.resetMapSelect(conn);
        this.resetGameModeSelect(conn);
    }

    // This is called on the server when a client disconnects.
    // NB! This can happen in two situations: in the lobby and in-game.
    public override void OnLobbyServerDisconnect(NetworkConnection conn) {
        base.OnLobbyServerDisconnect(conn);

        if (this._players.ContainsKey(conn.connectionId)) {
            this._players[conn.connectionId].placement = Mathf.Max(1, this.getNrOfPlayersAlive());
            this._players[conn.connectionId].isDead    = true;
        }

        this.resetMapSelect(conn);

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

    // This is called on the client when adding a player to the lobby fails.
    public override void OnLobbyClientAddPlayerFailed() {
        base.OnLobbyClientAddPlayerFailed();
        this.TryToAddPlayer();

        Debug.Log("NETWORK_LOBBY_MANAGER::OnLobbyClientAddPlayerFailed");
    }

    // This is called on the server when a player is removed.
    public override void OnLobbyServerPlayerRemoved(NetworkConnection conn, short playerControllerId) {
        base.OnLobbyServerPlayerRemoved(conn, playerControllerId);

        Debug.Log("NETWORK_LOBBY_MANAGER::OnLobbyServerPlayerRemoved:\n\tconn=" + conn + "\n\tplayerControllerId=" + playerControllerId);
    }

    // This allows customization of the creation of the lobby-player object on the server.
    public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId) {
        GameObject lobbyPlayerInstance = base.OnLobbyServerCreateLobbyPlayer(conn, playerControllerId);

        if (lobbyPlayerInstance == null)
            lobbyPlayerInstance = Instantiate(Resources.Load<GameObject>("Prefabs/Players/LobbyPlayer"));

        Debug.Log("NETWORK_LOBBY_MANAGER::OnLobbyServerCreateLobbyPlayer:\n\tconn=" + conn + "\n\tplayerControllerId=" + playerControllerId + "\n\tlobbyPlayerInstance=" + lobbyPlayerInstance);

        return lobbyPlayerInstance;
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

        Vector3           position       = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
        int               selectedModel  = this._players[conn.connectionId].animal;
        GameObject        playerPrefab   = Resources.Load<GameObject>("Prefabs/Players/" + this._models[selectedModel]);
        GameObject        playerInstance = Instantiate(playerPrefab, position, playerPrefab.transform.rotation);
        PlayerInformation playerInfo     = playerInstance.GetComponent<PlayerInformation>();

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
            case (short)NetworkMessageType.MSG_ATTACK:
                this.recieveAttackMessage(message);
                break;
            case (short)NetworkMessageType.MSG_LOBBY_UPDATE:
                this.recieveLobbyUpdateMessage();
                break;
            case (short)NetworkMessageType.MSG_MATCH_DROP:
                this.recieveMatchDropMessage();
                break;
            case (short)NetworkMessageType.MSG_MAP_SELECT:
                this.recieveMapSelectMessage(message);
                break;
            case (short)NetworkMessageType.MSG_GAMEMODE_SELECT:
                this.recieveGameModeSelectMessage(message);
                break;
            case (short)NetworkMessageType.MSG_DATA_FILE_LOADING:
                this.recieveDataFileMessage(message.conn.connectionId, true);
                break;
            case (short)NetworkMessageType.MSG_DATA_FILE_PROGRESS:
                this.recieveDataFileProgressMessage(message);
                break;
            case (short)NetworkMessageType.MSG_DATA_FILE_READY:
                this.recieveDataFileMessage(message.conn.connectionId, false);
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

    // Parse the attack message, and tell the victim to apply damage to itself.
    private void recieveAttackMessage(NetworkMessage message) {
        this.sendAttackMessage(message.ReadMessage<AttackMessage>());
    }

    // Parse the player select message, and select the player model.
    private void recievePlayerSelectMessage(NetworkMessage message) {
        this._players[message.conn.connectionId].animal = message.ReadMessage<IntegerMessage>().value;

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

    private void recieveMapSelectMessage(NetworkMessage message) {
        string map = message.ReadMessage<StringMessage>().value;

        if (this._mapVotes.ContainsKey(message.conn))
            this._mapVotes[message.conn] = map;
        else
            this._mapVotes.Add(message.conn, map);

        this.sendMapVotes();
    }

    private void recieveGameModeSelectMessage(NetworkMessage message) {
        string gamemode = message.ReadMessage<StringMessage>().value;
        Debug.Log(gamemode);

        if (this._gamemodeVotes.ContainsKey(message.conn))
            this._gamemodeVotes[message.conn] = gamemode;
        else
            this._gamemodeVotes.Add(message.conn, gamemode);

        this.sendGameModeVotes();
    }

    private void recieveDataFileMessage(int id, bool loading) {
        if (this._isLoading.ContainsKey(id))
            this._isLoading[id].loading = loading;
        else
            this._isLoading.Add(id, new LoadingPlayer());

        // Tell all the clients to wait until loading is complete.
        if (loading) {
            foreach (var conn in NetworkServer.connections) {
                if (conn != null)
                    this.sendDataFileMessage(conn.connectionId, true);
            }
        }

        // Tell all the clients to start after loading is complete.
        if (this.IsDataLoadingComplete()) {
            foreach (var conn in NetworkServer.connections) {
                if (conn != null)
                    this.sendDataFileMessage(conn.connectionId, false);
            }
        }
    }

    private void recieveDataFileProgressMessage(NetworkMessage message) {
        int progress = message.ReadMessage<IntegerMessage>().value;

        // Select the lowest progress value to make sure everyone waits for the slowest client.
        foreach (var client in this._isLoading) {
            if ((client.Value.progress < progress) && (client.Value.progress > 0))
                progress = client.Value.progress;
        }

        foreach (var conn in NetworkServer.connections) {
            if (conn != null)
                this.sendDataFileProgressMessage(conn.connectionId, progress);
        }
    }

    private void resetMapSelect(NetworkConnection conn) {
        if (this._mapVotes.ContainsKey(conn))
            this._mapVotes.Remove(conn);

        this.sendMapVotes();
    }

    private void sendMapVotes() {
        var votes = this.getVotes(this._mapVotes);
        string message = "";

        foreach (var vote in votes)
            message += "|" + vote.Key + ":" + vote.Value;            

        NetworkServer.SendToAll((short)NetworkMessageType.MSG_MAP_VOTE, new StringMessage(message));
    }

    Dictionary<string, int> getVotes(Dictionary<NetworkConnection, string> votes) {
        Dictionary<string, int> _votes = new Dictionary<string, int>();

        foreach (var vote in votes.Values) {
            if (_votes.ContainsKey(vote))
                _votes[vote] += 1;
            else
                _votes.Add(vote, 1);
        }

        return _votes;
    }

    //Returns the map with the most votes, if theres multiple winners one winner is chosen at random
    public string getMap() {
        List<string> winnerMaps = new List<string>();
        var votes = getVotes(this._mapVotes);

        // Votes (select the top voted island)
        if (votes.Count > 0) {
            int maxVotes = 0;

            foreach(int voteCount in votes.Values) {
                if (maxVotes < voteCount)
                    maxVotes = voteCount;
            }

            foreach (var vote in votes) {
                if (vote.Value == maxVotes)
                    winnerMaps.Add(vote.Key);
            }
        // No votes (select from all available islands)
        } else {
            foreach (string island in this._islands)
                winnerMaps.Add(island);
        }

        this._mapVotes = new Dictionary<NetworkConnection, string>(); //Clear vote data

        return winnerMaps[Random.Range(0, winnerMaps.Count)];
    }

    private void resetGameModeSelect(NetworkConnection conn) {
        if (this._gamemodeVotes.ContainsKey(conn))
            this._gamemodeVotes.Remove(conn);

        this.sendGameModeVotes();
    }

    private void sendGameModeVotes() {
        var votes = this.getVotes(this._gamemodeVotes);
        string message = "";

        foreach (var vote in votes)
            message += "|" + vote.Key + ":" + vote.Value;
        Debug.Log(message);
        NetworkServer.SendToAll((short)NetworkMessageType.MSG_GAMEMODE_VOTE, new StringMessage(message));
    }

    //Returns the gameMode with the most votes, if theres multiple winners one winner is chosen at random
    public string getGameMode() {
        List<string> winnerGameModes = new List<string>();
        var votes = getVotes(this._gamemodeVotes);

        // Votes (select the top voted island)
        if (votes.Count > 0) {
            int maxVotes = 0;

            foreach (int voteCount in votes.Values) {
                if (maxVotes < voteCount)
                    maxVotes = voteCount;
            }

            foreach (var vote in votes) {
                if (vote.Value == maxVotes)
                    winnerGameModes.Add(vote.Key);
            }
            // No votes (select from all available islands)
        } else {
            foreach (string island in this._islands)
                winnerGameModes.Add(island);
        }

        this._mapVotes = new Dictionary<NetworkConnection, string>(); //Clear vote data

        return winnerGameModes[Random.Range(0, winnerGameModes.Count)];
    }

    // Tell the client the state of data file loading.
    private void sendDataFileMessage(int id, bool loading) {
        if (loading)
            NetworkServer.SendToClient(id, (short)NetworkMessageType.MSG_DATA_FILE_LOADING, new IntegerMessage());
        else
            NetworkServer.SendToClient(id, (short)NetworkMessageType.MSG_DATA_FILE_READY, new IntegerMessage());
    }

    // Tell the client the progress of data file loading.
    private void sendDataFileProgressMessage(int id, int progress) {
        NetworkServer.SendToClient(id, (short)NetworkMessageType.MSG_DATA_FILE_PROGRESS, new IntegerMessage(progress));
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

    // Tell the victim to apply damage to itself, and who the attacker was.
    private void sendAttackMessage(AttackMessage message) {
        if (this._players.ContainsKey(message.victimID))
            NetworkServer.SendToClient(message.victimID, (short)NetworkMessageType.MSG_ATTACK, message);
    }

    // Tell the player the end game stats.
    private void sendGameOverMessage(int id, int killerID = -1, bool win = true) {
        GameOverMessage message = new GameOverMessage();

        switch ((KillerID)killerID) {
            case KillerID.KILLER_ID_FALL:  message.killer = "KILLER_ID_FALL"; break;
            case KillerID.KILLER_ID_WALL:  message.killer = "KILLER_ID_WALL"; break;
            case KillerID.KILLER_ID_WATER: message.killer = "KILLER_ID_WATER"; break;
            default:                       message.killer = (killerID >= 0 ? this._players[killerID].name : ""); break;
        }

        message.name      = this._players[id].name;
        message.animal    = this._players[id].animal;
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
            message.players[i].animal = players[i].animal;
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
            //player.Value.score  = 0;
            //player.Value.score += ((7 - player.Value.placement) * 1000);
            player.Value.score  = (((this._players.Count + 1) - player.Value.placement) * 1000);
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
            message.rankings[i].animal = rankings[i].animal;
            message.rankings[i].isDead = rankings[i].isDead;
            message.rankings[i].rank   = (i + 1);
            message.rankings[i].kills  = rankings[i].kills;
            message.rankings[i].score  = rankings[i].score;
            message.rankings[i].win    = rankings[i].win;

            // Only send the scores once from the server.
            if (id == NetworkServer.serverHostId)
                Leaderboard.SaveScore(rankings[i].name, (double)rankings[i].score);
        }

        if (id == NetworkServer.serverHostId) {
            Leaderboard.SaveStats(this._models[rankings[0].animal]);

            // DEBUG: Print stats to console
            for (int i = 0; i < this._models.Length; i++)
                Leaderboard.GetStats(this._models[i]);
        }

        NetworkServer.SendToClient(id, (short)NetworkMessageType.MSG_RANKINGS, message);
    }
}
