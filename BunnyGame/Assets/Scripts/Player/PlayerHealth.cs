using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour {

    private const float     MAX_HEALTH = 100;
    private NetworkClient _client;
    private Image         _damageImage;
    private bool          _damageImmune;
    private Text          _gameOverText;
    private bool          _isDead;
    private Button        _spectateButton;
    private Image         _spectateImage;
    private Text          _spectateText;
    private bool          _winner;

    [SyncVar(hook = "showGameOverScreen")]
    private GameOverMessage _gameOver = new GameOverMessage();

    [SyncVar(hook = "updatePlayerStatsText")]
    private PlayerStatsMessage _playerStats = new PlayerStatsMessage();

    [SyncVar(hook = "updateDamageScreen")]
    private float _currentHealth = MAX_HEALTH;

    void Start() {
        if (!this.isLocalPlayer)
            return;

        this._damageImage    = GameObject.Find("Canvas/BloodSplatterOverlay").GetComponent<Image>();
        this._gameOverText   = GameObject.Find("Canvas/GameOverText").GetComponent<Text>();
        this._spectateButton = GameObject.Find("Canvas/SpectateButton").GetComponent<Button>();
        this._spectateImage  = GameObject.Find("Canvas/SpectateButton").GetComponent<Image>();
        this._spectateText   = GameObject.Find("Canvas/SpectateButton/SpectateButtonText").GetComponent<Text>();
        this._isDead         = false;
        this._winner         = false;
        this._client         = NetworkClient.allClients[0];

        if (this._client != null) {
            this._client.RegisterHandler((short)NetworkMessageType.MSG_PLAYER_STATS, this.recieveNetworkMessage);
            this._client.RegisterHandler((short)NetworkMessageType.MSG_GAME_OVER,    this.recieveNetworkMessage);

            this._client.Send((short)NetworkMessageType.MSG_PLAYER_READY, new IntegerMessage());
        }

        // Make player immune from damage for the first 5 seconds after spawning
        StartCoroutine(this.damageImmune(5.0f));
    }

    // Make the player immune from damage for <seconds>
    private IEnumerator damageImmune(float seconds) {
        this._damageImmune = true;
        yield return new WaitForSeconds(seconds);
        this._damageImmune = false;
    }

    // Check if the player is dead.
    public bool IsDead() {
        return this._isDead;
    }

    // TODO: Implement spectating
    private void spectate() {
        Debug.Log("TODO: Implement spectating");
    }

    //
    // Networked methods, runs on the server or client appropriately.
    //

    // Take damage when hit, and respawn the player if dead.
    public void TakeDamage(float amount, int connectionID) {
        if (!this.isLocalPlayer || this._damageImmune)
            return;

        if (this.isServer && this.isClient)
            this.takeDamage2(amount, connectionID);
        else if (this.isServer)
            this.RpcTakeDamage(amount, connectionID);
        else if (this.isClient)
            this.CmdTakeDamage(amount, connectionID);
    }

    private void takeDamage2(float amount, int connectionID) {
        if (this._currentHealth <= 0)
            return;

        this._currentHealth -= amount;
        this._isDead         = (this._currentHealth <= 0);

        if (this._isDead)
            this.Die(connectionID);
    }

    [ClientRpc]
    private void RpcTakeDamage(float amount, int connectionID) {
        this.takeDamage2(amount, connectionID);
    }

    [Command]
    private void CmdTakeDamage(float amount, int connectionID) {
        this.RpcTakeDamage(amount, connectionID);
    }

    // Respawn the player in a new random position.
    private void Die(int connectionID) {
        if (this.isServer)
            this.RpcDie();
        else if (this.isClient)
            this.CmdDie();

        if (this._client != null)
            this._client.Send((short)NetworkMessageType.MSG_KILLER_ID, new IntegerMessage(connectionID));
    }

    [ClientRpc]
    private void RpcDie() {
        this.gameObject.transform.GetChild(1).gameObject.SetActive(false);
    }

    [Command]
    private void CmdDie() {
        this.RpcDie();
    }

    //
    // Network message handling, sends/receieves messages between clients and server.
    //

    // Recieve and handle the network message.
    private void recieveNetworkMessage(NetworkMessage message) {
        if (!this.isLocalPlayer)
            return;

        switch (message.msgType) {
            case (short)NetworkMessageType.MSG_PLAYER_STATS:
                this.updatePlayerStats(message.ReadMessage<PlayerStatsMessage>());
                break;
            case (short)NetworkMessageType.MSG_GAME_OVER:
                this.updateGameOver(message.ReadMessage<GameOverMessage>());
                break;
            default:
                Debug.Log("ERROR! Unknown message type: " + message.msgType);
                break;
        }
    }

    // Update the player stats.
    private void updatePlayerStats(PlayerStatsMessage message) {
        if (this.isServer && this.isClient)
            this.updatePlayerStats2(message);
        else if (this.isServer)
            this.RpcUpdatePlayerStats(message);
        else if (this.isClient)
            this.CmdUpdatePlayerStats(message);
    }

    private void updatePlayerStats2(PlayerStatsMessage message) {
        if (message.playersAlive > 0)
            this._playerStats = message;
    }

    [ClientRpc]
    private void RpcUpdatePlayerStats(PlayerStatsMessage message) {
        this.updatePlayerStats2(message);
    }

    [Command]
    private void CmdUpdatePlayerStats(PlayerStatsMessage message) {
        this.RpcUpdatePlayerStats(message);
    }

    // Update the player ranking.
    private void updateGameOver(GameOverMessage message) {
        if (this.isServer && this.isClient)
            this.updateGameOver2(message);
        else if (this.isServer)
            this.RpcUpdateGameOver(message);
        else if (this.isClient)
            this.CmdUpdateGameOver(message);
    }

    private void updateGameOver2(GameOverMessage message) {
        this._gameOver = message;
    }

    [ClientRpc]
    private void RpcUpdateGameOver(GameOverMessage message) {
        this.updateGameOver2(message);
    }

    [Command]
    private void CmdUpdateGameOver(GameOverMessage message) {
        this.RpcUpdateGameOver(message);
    }

    //
    // Synchronized methods, runs on the clients when the variables are changed on the server.
    //

    // The game has ended, show the win or death screen respectively.
    private void showGameOverScreen(GameOverMessage message) {
        if (!this.isLocalPlayer)
            return;

        if (message.win)
            this.showWinScreen(message);
        else
            this.showDeathScreen(message);

        StartCoroutine(gameOverTimer(10));
    }


    // Show the win screen.
    private void showWinScreen(GameOverMessage message) {
		if ((this._gameOverText == null) || !message.win)
            return;

        this._winner             = true;
        this._gameOver           = message;
        this._gameOverText.text  = string.Format("WINNER WINNER {0} DINNER!\nKills: {1}   Rank: #1", this._gameOver.name, this._gameOver.kills);
        this._gameOverText.color = new Color(this._gameOverText.color.r, this._gameOverText.color.g, this._gameOverText.color.b, 1.0f);

        this.showRankings(message);
    }

    // Show the death screen.
    private void showDeathScreen(GameOverMessage message) {
		if ((this._gameOverText == null) || (this._spectateImage == null) || (this._spectateText == null) || message.win)
            return;

        if (this._winner)
            return;

        this._gameOver = message;

        if (this._gameOver.killer != "")
            this._gameOverText.text = string.Format("YOU WERE KILLED BY {0}\nKills: {1}   Rank: #{2}", this._gameOver.killer, this._gameOver.kills, this._gameOver.rank);
        else
            this._gameOverText.text = string.Format("YOU DIED\nKills: {0}   Rank: #{1}", this._gameOver.kills, this._gameOver.rank);

        this._gameOverText.color  = new Color(this._gameOverText.color.r,  this._gameOverText.color.g,  this._gameOverText.color.b,  1.0f);
        this._spectateImage.color = new Color(this._spectateImage.color.r, this._spectateImage.color.g, this._spectateImage.color.b, 1.0f);
        this._spectateText.color  = new Color(this._spectateText.color.r,  this._spectateText.color.g,  this._spectateText.color.b,  1.0f);

        this.showRankings(message);

        this._spectateButton.onClick.AddListener(this.spectate); // TODO: see spectate method
    }


    // Shows a countdown until you are automatically moved out of the server
    private IEnumerator gameOverTimer(float time) {

        // Wait until all but one player is dead
        while (this._playerStats.playersAlive > 1) {
            yield return new WaitForSeconds(0.1f);
        }

        string message = "Sending you back to lobby in: ";

        GameObject timeDisplay = GameObject.Find("ConstantSizeCanvas").transform.GetChild(2).gameObject;
        Text timeDisplayText = timeDisplay.GetComponent<Text>();
        timeDisplayText.text = message + time.ToString("0");
        timeDisplay.SetActive(true);

        float timeremaining = time;
        while (timeremaining > 0) {
            timeDisplayText.text = message + timeremaining.ToString("0");
            timeremaining -= Time.deltaTime;
            yield return null;
        }

        SceneManager.LoadScene("Lobby"); // Not sure if anything else should be done before leaving the scene?
    }

    // Show a message saying who killed who that fades away over time.
    private IEnumerator showKilledText(float totalSeconds, float passedSeconds, Text killedText) {
        Color startColor = new Color(killedText.color.r, killedText.color.g, killedText.color.b, 1.0f);
        Color endColor   = new Color(killedText.color.r, killedText.color.g, killedText.color.b, 0.0f);
        killedText.color = Color.Lerp(startColor, endColor, (passedSeconds / totalSeconds));

        yield return new WaitForSeconds(0.01f);

        if (passedSeconds < totalSeconds)
            StartCoroutine(this.showKilledText(5.0f, (passedSeconds + 0.01f), killedText));
    }

    // Show a list of all the player rankings and stats.
    private void showRankings(GameOverMessage message) {

        //foreach (Player player in message.rankings) {
        //    print("#TEST: name=" + player.name + " - rank=" + player.rank + " - winner=" + player.win);
        //}

    }

    // Update the HUD showing the player stats.
    private void updatePlayerStatsText(PlayerStatsMessage message) {
        if (!this.isLocalPlayer)
            return;

        Text aliveText = GameObject.Find("Canvas/PlayerStatsText").GetComponent<Text>();

        if ((aliveText == null) || (message.playersAlive < 1))
            return;

        this._playerStats = message;
        aliveText.text    = string.Format("{0}\n{1} ALIVE\n{2} KILLS", this._playerStats.name, this._playerStats.playersAlive, this._playerStats.kills);

        if ((this._playerStats.killer == "") || (this._playerStats.dead == ""))
            return;

        Text killedText = GameObject.Find("Canvas/PlayerKilledText").GetComponent<Text>();

        if (killedText != null) {
            killedText.text = string.Format("{0} was killed by {1}", this._playerStats.dead, this._playerStats.killer);
            StartCoroutine(this.showKilledText(5.0f, 0.0f, killedText));
        }
    }

    // Update the health/damage screen overlay.
    private void updateDamageScreen(float health) {
        if (!this.isLocalPlayer || (this._damageImage == null) || (this._damageImage.color.a >= 1.0f) || (health < 0))
            return;

        float alpha             = (1.0f - health / MAX_HEALTH);
        this._damageImage.color = new Color(this._damageImage.color.r, this._damageImage.color.g, this._damageImage.color.b, alpha);
    }
}
