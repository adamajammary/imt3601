using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour {

    private const int     MAX_HEALTH = 100;
    private NetworkClient _client;
    private Image         _damageImage;
    private bool          _damageImmune;
    private Text          _gameOverText;
    private bool          _isDead;
    private Button        _spectateButton;
    private Image         _spectateImage;
    private Text          _spectateText;

    [SyncVar(hook = "showGameOverScreen")] private int _rank          = -1;
    [SyncVar(hook = "updateAliveText")]    private int _playersAlive  = -2;
    [SyncVar(hook = "updateDamageScreen")] private int _currentHealth = MAX_HEALTH;

    void Start() {
        if (!this.isLocalPlayer)
            return;

        this._damageImage    = GameObject.Find("Canvas/BloodSplatterOverlay").GetComponent<Image>();
        this._gameOverText   = GameObject.Find("Canvas/PlayerIsDeadText").GetComponent<Text>();
        this._spectateButton = GameObject.Find("Canvas/SpectateButton").GetComponent<Button>();
        this._spectateImage  = GameObject.Find("Canvas/SpectateButton").GetComponent<Image>();
        this._spectateText   = GameObject.Find("Canvas/SpectateButton/SpectateButtonText").GetComponent<Text>();
        this._isDead         = false;
        this._client         = NetworkClient.allClients[0];

        if (this._client != null) {
            this._client.RegisterHandler((short)NetworkMessageType.MSG_PLAYERCOUNT, this.recieveNetworkMessage);
            this._client.RegisterHandler((short)NetworkMessageType.MSG_PLAYERDIED,  this.recieveNetworkMessage);
            this._client.RegisterHandler((short)NetworkMessageType.MSG_PLAYERWON,   this.recieveNetworkMessage);
        }

        this.requestAliveCounter();

        // Make player immune from damage for the first 5 seconds after spawning
        StartCoroutine(this.damageImmune(5.0f));
    }

    // Make the player immune from damage for <seconds>
    private IEnumerator damageImmune(float seconds) {
        this._damageImmune = true;
        yield return new WaitForSeconds(seconds);
        this._damageImmune = false;
    }

    // TODO: Implement spectating
    private void spectate() {
        Debug.Log("TODO: Implement spectating");
    }

    //
    // Networked methods, runs on the server or client appropriately.
    //

    // Check if player is dead.
    public bool IsDead() {
        //if (this.isServer && this.isClient)
        //    this.isDead2();
        //if (this.isServer)
        //    this.RpcIsDead();
        //else if (this.isClient)
        //    this.CmdIsDead();

        return (!this.gameObject.transform.GetChild(1).gameObject.activeSelf && !this.gameObject.transform.GetChild(1).gameObject.activeInHierarchy);
        //return this._isDead;
    }

    //private void isDead2() {
    //    //print("isDead2_1: " + this._isDead);
    //    //this._isDead = (this._currentHealth <= 0);
    //    this._isDead = (!this.gameObject.transform.GetChild(1).gameObject.activeSelf && !this.gameObject.transform.GetChild(1).gameObject.activeInHierarchy);
    //    //print("isDead2_2: " + this._isDead);
    //}

    //[ClientRpc]
    //private void RpcIsDead() {
    //    //this._isDead = (this._currentHealth <= 0);
    //    this._isDead = (!this.gameObject.transform.GetChild(1).gameObject.activeSelf && !this.gameObject.transform.GetChild(1).gameObject.activeInHierarchy);
    //    //this.isDead2();
    //}

    //[Command]
    //private void CmdIsDead() {
    //    this.RpcIsDead();
    //}

    // Take damage when hit, and respawn the player if dead.
    public void TakeDamage(int amount) {
        if (!this.isLocalPlayer || this._damageImmune || this.IsDead())
            return;

        if (this.isServer && this.isClient)
            this.takeDamage2(amount);
        else if (this.isServer)
            this.RpcTakeDamage(amount);
        else if (this.isClient)
            this.CmdTakeDamage(amount);
    }

    private void takeDamage2(int amount) {
        this._currentHealth -= amount;
        this._isDead         = (this._currentHealth <= 0);

        if (this._isDead)
            this.Die();
    }

    [ClientRpc]
    private void RpcTakeDamage(int amount) {
        this.takeDamage2(amount);
    }

    [Command]
    private void CmdTakeDamage(int amount) {
        this.RpcTakeDamage(amount);
    }

    // Respawn the player in a new random position.
    private void Die() {
        if (this.isServer)
            this.RpcDie();
        else if (this.isClient)
            this.CmdDie();

        if (this._client != null)
            this._client.Send((short)NetworkMessageType.MSG_PLAYERDIED, new IntegerMessage());
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

    // Ask the server how many players are still alive.
    private void requestAliveCounter() {
        if (this.isLocalPlayer && (this._client != null))
            this._client.Send((short)NetworkMessageType.MSG_PLAYERCOUNT, new IntegerMessage());
    }

    // Recieve and handle the network message.
    private void recieveNetworkMessage(NetworkMessage message) {
        if (!this.isLocalPlayer)
            return;

        switch (message.msgType) {
            case (short)NetworkMessageType.MSG_PLAYERCOUNT:
                this.updateAliveCounter(message.ReadMessage<IntegerMessage>().value);
                break;
            case (short)NetworkMessageType.MSG_PLAYERDIED:
            case (short)NetworkMessageType.MSG_PLAYERWON:
                this.updateRanking(message.ReadMessage<IntegerMessage>().value);
                break;
            default:
                Debug.Log("ERROR! Unknown message type: " + message.msgType);
                break;
        }
    }

    // Update the number of players still alive.
    private void updateAliveCounter(int players) {
        if (this.isServer && this.isClient)
            this.updateAliveCounter2(players);
        else if (this.isServer)
            this.RpcUpdateAliveCounter(players);
        else if (this.isClient)
            this.CmdUpdateAliveCounter(players);
    }

    private void updateAliveCounter2(int players) {
        this._playersAlive = players;
    }

    [ClientRpc]
    private void RpcUpdateAliveCounter(int players) {
        this.updateAliveCounter2(players);
    }

    [Command]
    private void CmdUpdateAliveCounter(int players) {
        this.RpcUpdateAliveCounter(players);
    }

    // Update the player ranking.
    private void updateRanking(int rank) {
        if (this.isServer && this.isClient)
            this.updateRanking2(rank);
        else if (this.isServer)
            this.RpcUpdateRanking(rank);
        else if (this.isClient)
            this.CmdUpdateRanking(rank);
    }

    private void updateRanking2(int rank) {
        this._rank = rank;
    }

    [ClientRpc]
    private void RpcUpdateRanking(int rank) {
        this.updateRanking2(rank);
    }

    [Command]
    private void CmdUpdateRanking(int rank) {
        this.RpcUpdateRanking(rank);
    }

    //
    // Synchronized methods, runs on the clients when the variables are changed on the server.
    //

    // The game has ended, show the win or death screen respectively.
    private void showGameOverScreen(int rank) {
        if (rank > 1)
            this.showDeathScreen(rank);
        else
            this.showWinScreen(rank);
    }

    // Show the death screen.
    private void showDeathScreen(int rank) {
		if ((this._gameOverText == null) || (this._spectateImage == null) || (this._spectateText == null))
            return;

        int kills = 0;

        this._rank                = rank;
        this._gameOverText.text   = string.Format("YOU'RE DEAD\nKills: {0}   Rank: #{1}", kills, this._rank);
        this._gameOverText.color  = new Color(this._gameOverText.color.r,  this._gameOverText.color.g,  this._gameOverText.color.b,  1.0f);
        this._spectateImage.color = new Color(this._spectateImage.color.r, this._spectateImage.color.g, this._spectateImage.color.b, 1.0f);
        this._spectateText.color  = new Color(this._spectateText.color.r,  this._spectateText.color.g,  this._spectateText.color.b,  1.0f);

        this._spectateButton.onClick.AddListener(this.spectate); // TODO: see spectate method below
    }

    // Show the win screen.
    private void showWinScreen(int rank) {
		if (this._gameOverText == null)
            return;

        int kills = 0;

        this._rank               = rank;
        this._gameOverText.text  = string.Format("YOU WON\nKills: {0}   Rank: #{1}", kills, this._rank);
        this._gameOverText.color = new Color(this._gameOverText.color.r, this._gameOverText.color.g, this._gameOverText.color.b, 1.0f);
    }

    // Update the HUD showing the number of players still alive.
    private void updateAliveText(int playersAlive) {
        Text aliveText = GameObject.Find("Canvas/PlayersAliveText").GetComponent<Text>();

        if (aliveText != null) {
            this._playersAlive = playersAlive;
            aliveText.text     = string.Format("{0} ALIVE", this._playersAlive);
        }
    }

    // Update the health/damage screen overlay.
    private void updateDamageScreen(int health) {
        if (!this.isLocalPlayer || (this._damageImage == null) || (this._damageImage.color.a >= 1.0f))
            return;

        float alpha             = (1.0f - (float)health / (float)MAX_HEALTH);
        this._damageImage.color = new Color(this._damageImage.color.r, this._damageImage.color.g, this._damageImage.color.b, alpha);
    }
}
