using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour {

    private const float MAX_HEALTH     = 100;
    private float       _currentHealth = MAX_HEALTH;
    private Image       _damageImage   = null;
    private bool        _damageImmune  = false;
    private Text        _gameOverText  = null;
    private bool        _isDead        = false;
    private Image       _spectateImage = null;
    private Text        _spectateText  = null;
    private bool        _ranked        = false;
    private bool        _winner        = false;

    void Start() {
        if (!this.isLocalPlayer || (SceneManager.GetActiveScene().name == "Lobby"))
            return;

        this._damageImage   = GameObject.Find("Canvas/BloodSplatterOverlay").GetComponent<Image>();
        this._gameOverText  = GameObject.Find("Canvas/GameOverText").GetComponent<Text>();
        this._spectateImage = GameObject.Find("Canvas/SpectateButton").GetComponent<Image>();
        this._spectateText  = GameObject.Find("Canvas/SpectateButton/SpectateButtonText").GetComponent<Text>();

        if (NetworkClient.allClients[0] != null) {
            NetworkClient.allClients[0].RegisterHandler((short)NetworkMessageType.MSG_ATTACK,       this.recieveNetworkMessage);
            NetworkClient.allClients[0].RegisterHandler((short)NetworkMessageType.MSG_PLAYER_STATS, this.recieveNetworkMessage);
            NetworkClient.allClients[0].RegisterHandler((short)NetworkMessageType.MSG_GAME_OVER,    this.recieveNetworkMessage);
            NetworkClient.allClients[0].RegisterHandler((short)NetworkMessageType.MSG_RANKINGS,     this.recieveNetworkMessage);

            NetworkClient.allClients[0].Send((short)NetworkMessageType.MSG_PLAYER_READY, new IntegerMessage());
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

    // Start spectating
    private void spectate() {
        GameObject.Find("Main Camera").GetComponent<SpectatorController>().startSpectating();

        GetComponent<PlayerController>().enabled = false;
        GetComponent<Collider>().enabled = false;
        GetComponent<PlayerEffects>().enabled = false;

        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(false);
    }

    //
    // Networked methods, runs on the server or client appropriately.
    //

    public void Attack(float amount, GameObject attacker, int attackerID, int victimID, Vector3 impactPos) {
        PlayerEffects playerEffects = this.gameObject.GetComponent<PlayerEffects>();

        if ((playerEffects != null) && !this._damageImmune) {
            AttackMessage message = new AttackMessage();

            message.damageAmount   = playerEffects.calcDamage(attacker, amount);
            message.attackerID     = attackerID;
            message.victimID       = victimID;
            message.impactPosition = impactPos;

            NetworkClient.allClients[0].Send((short)NetworkMessageType.MSG_ATTACK, message);
        }
    }

    public void maxHeal() {
        Heal(MAX_HEALTH - this._currentHealth);
    }

    public void Heal(float amount) {
        if (this.isServer)
            this.RpcHeal(amount);
        else if (this.isClient)
            this.CmdHeal(amount);
    }

    [ClientRpc]
    private void RpcHeal(float amount) {
        this.updateDamageScreen(-amount);
    }

    [Command]
    private void CmdHeal(float amount) {
        this.RpcHeal(amount);
    }

    public void TakeDamage(float amount, int attackerID) {
        if (this.isServer)
            this.RpcTakeDamage(amount, attackerID);
        else if (this.isClient)
            this.CmdTakeDamage(amount, attackerID);
                
        GameObject.Find("MainCamera").GetComponent<CameraShake>().isShaking = true;

    }

    [ClientRpc]
    private void RpcTakeDamage(float amount, int attackerID) {
        if (this._isDead)
            return;

        this.updateDamageScreen(amount);

        if (this._isDead)
            this.die(attackerID);

        
    }

    [Command]
    private void CmdTakeDamage(float amount, int attackerID) {
        this.RpcTakeDamage(amount, attackerID);
    }



    //
    // Network message handling, sends/receieves messages between clients and server.
    //

    // Recieve and handle the network message.
    private void recieveNetworkMessage(NetworkMessage message) {
        switch (message.msgType) {
            case (short)NetworkMessageType.MSG_ATTACK:
                this.applyDamage(message.ReadMessage<AttackMessage>());
                break;
            case (short)NetworkMessageType.MSG_PLAYER_STATS:
                this.updatePlayerStats(message.ReadMessage<PlayerStatsMessage>());
                break;
            case (short)NetworkMessageType.MSG_GAME_OVER:
                this.updateGameOver(message.ReadMessage<GameOverMessage>());
                break;
            case (short)NetworkMessageType.MSG_RANKINGS:
                this.updateRankings(message.ReadMessage<RankingsMessage>());
                break;
            default:
                Debug.Log("ERROR! Unknown message type: " + message.msgType);
                break;
        }
    }

    // Apply damage.
    private void applyDamage(AttackMessage message) {
        if (this.isServer)
            this.RpcApplyDamage(message);
        else if (this.isClient)
            this.CmdApplyDamage(message);
    }

    [ClientRpc]
    private void RpcApplyDamage(AttackMessage message) {
        PlayerEffects playerEffects = this.gameObject.GetComponent<PlayerEffects>();

        if ((playerEffects != null) && !this._isDead && !this._damageImmune) {
            this.updateDamageScreen(message.damageAmount);
            playerEffects.CmdBloodParticle(message.impactPosition);

            if (this._isDead)
                this.die(message.attackerID);
        }
    }

    [Command]
    private void CmdApplyDamage(AttackMessage message) {
        this.RpcApplyDamage(message);
    }
    
    // Kill the player.
    private void die(int killerID) {
        if (this.isServer)
            this.RpcDie(killerID);
        else if (this.isClient)
            this.CmdDie(killerID);

        if ((NetworkClient.allClients[0] != null) && this.isLocalPlayer) {
            NetworkClient.allClients[0].Send((short)NetworkMessageType.MSG_KILLER_ID, new IntegerMessage(killerID));
            GetComponent<PlayerAbilityManager>().sendAbilitiesToKiller(killerID);
        }
    }

    [ClientRpc]
    private void RpcDie(int killerID) {
        if (GameInfo.gamemode == "Battleroyale") {
            this.gameObject.transform.GetChild(1).gameObject.SetActive(false);

            GetComponent<PlayerController>().enabled = false;
            GetComponent<Collider>().enabled = false;
            GetComponent<PlayerEffects>().enabled = false;

            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(false);

            this._isDead = true;
        }
        else if (GameInfo.gamemode == "Deathmatch") {
            GetComponent<PlayerController>().spawn();
            Debug.Log("DIE");
        }
    }

    [Command]
    private void CmdDie(int killerID) {
        this.RpcDie(killerID);
    }

    // Update the player stats.
    private void updatePlayerStats(PlayerStatsMessage message) {
        if (this.isServer)
            this.RpcUpdatePlayerStats(message);
        else if (this.isClient)
            this.CmdUpdatePlayerStats(message);
    }

    [ClientRpc]
    private void RpcUpdatePlayerStats(PlayerStatsMessage message) {
        this.updatePlayerStatsText(message);
    }

    [Command]
    private void CmdUpdatePlayerStats(PlayerStatsMessage message) {
        this.RpcUpdatePlayerStats(message);
    }

    // Update the player ranking.
    private void updateGameOver(GameOverMessage message) {
        if (this.isServer)
            this.RpcUpdateGameOver(message);
        else if (this.isClient)
            this.CmdUpdateGameOver(message);
    }

    [ClientRpc]
    private void RpcUpdateGameOver(GameOverMessage message) {
        this.showGameOverScreen(message);
    }

    [Command]
    private void CmdUpdateGameOver(GameOverMessage message) {
        this.RpcUpdateGameOver(message);
    }

    // Update the game rankings.
    private void updateRankings(RankingsMessage message) {
        if (this.isServer)
            this.RpcUpdateRankings(message);
        else if (this.isClient)
            this.CmdUpdateRankings(message);
    }

    [ClientRpc]
    private void RpcUpdateRankings(RankingsMessage message) {
        this.showRankings(message);
    }

    [Command]
    private void CmdUpdateRankings(RankingsMessage message) {
        this.RpcUpdateRankings(message);
    }

    //
    // GUI methods, updates and displays information on the screen.
    //

    // The game has ended, show the win or death screen respectively.
    private void showGameOverScreen(GameOverMessage message) {
        if (!this.isLocalPlayer)
            return;

        if (message.win)
            this.showWinScreen(message);
        else
            this.showDeathScreen(message);
    }

    // Show the win screen.
    private void showWinScreen(GameOverMessage message) {
        if ((this._gameOverText == null) || !message.win || this._winner)
            return;

        string animal = "";

        switch (message.animal) {
            case 0:  animal = "BUNNY"; break;
            case 1:  animal = "FOXY";  break;
            case 2:  animal = "BIRDY"; break;
            case 3:  animal = "MOOZY"; break;
            default: Debug.Log("ERROR! Unknown model: " + message.animal); break;
        }

        this._winner             = true;
        this._gameOverText.text  = string.Format("WINNER WINNER {0} DINNER!\n\tPlacement: #1\t\tKills: {2}\n", animal, message.placement, message.kills);
        this._gameOverText.color = new Color(this._gameOverText.color.r, this._gameOverText.color.g, this._gameOverText.color.b, 1.0f);
    }

    // Show the death screen.
    private void showDeathScreen(GameOverMessage message) {
        if ((this._gameOverText == null) || (this._spectateImage == null) || (this._spectateText == null) || message.win || this._winner || GameInfo.gamemode == "Deathmatch")
            return;

        if (message.killer == "KILLER_ID_FALL")
            this._gameOverText.text = string.Format("YOU FELL TO YOUR DEATH!\n\tPlacement: #{0}\t\tKills: {1}\n", message.placement, message.kills);
        else if (message.killer == "KILLER_ID_WALL")
            this._gameOverText.text = string.Format("YOU WERE BURNED TO DEATH!\n\tPlacement: #{0}\t\tKills: {1}\n", message.placement, message.kills);
        else if (message.killer == "KILLER_ID_WATER")
            this._gameOverText.text = string.Format("YOU DROWNED!\n\tPlacement: #{0}\t\tKills: {1}\n", message.placement, message.kills);
        else if (message.killer != "")
            this._gameOverText.text = string.Format("YOU WERE KILLED BY {0}!\n\tPlacement: #{1}\t\tKills: {2}\n", message.killer, message.placement, message.kills);
        else
            this._gameOverText.text = string.Format("UNKNOWN CAUSE OF DEATH!\n\tPlacement: #{0}\t\tKills: {1}\n", message.placement, message.kills);

        this._gameOverText.color  = new Color(this._gameOverText.color.r,  this._gameOverText.color.g,  this._gameOverText.color.b,  1.0f);
        this._spectateImage.color = new Color(this._spectateImage.color.r, this._spectateImage.color.g, this._spectateImage.color.b, 1.0f);
        this._spectateText.color  = new Color(this._spectateText.color.r,  this._spectateText.color.g,  this._spectateText.color.b,  1.0f);

        this.spectate();
    }

    // Shows a countdown until you are automatically moved out of the server
    private IEnumerator gameOverTimer(float time) {
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

        NetworkManager.singleton.StopHost();
        SceneManager.LoadScene("Lobby");
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
    private void showRankings(RankingsMessage message) {
        if (!this.isLocalPlayer || (this._gameOverText == null) || this._ranked)
            return;

        if (GameInfo.gamemode == "Deathmatch") {
            this._gameOverText.color = new Color(this._gameOverText.color.r, this._gameOverText.color.g, this._gameOverText.color.b, 1.0f);
            this._gameOverText.text = "";
        }
        this._ranked             = true;
        this._gameOverText.text += "\nRank\tKills\t\tScore\tName\n";
        this._gameOverText.text += "---------------------------------------";
        foreach (Player player in message.rankings)
            this._gameOverText.text += string.Format("\n#{0}\t\t{1}\t\t\t{2}\t{3}", player.rank, player.kills, player.score, player.name);

        StartCoroutine(this.gameOverTimer(15));
    }

    // Update the HUD showing the player stats.
    private void updatePlayerStatsText(PlayerStatsMessage message) {
        if (!this.isLocalPlayer)
            return;

        Text aliveText = GameObject.Find("Canvas/PlayerStatsText").GetComponent<Text>();

        if ((aliveText == null) || (message.playersAlive < 1))
            return;

        aliveText.text = string.Format("{0}\n{1} ALIVE\n{2} KILLS", message.name, message.playersAlive, message.kills);

        if ((message.killer == "") || (message.dead == ""))
            return;

        Text killedText = GameObject.Find("Canvas/PlayerKilledText").GetComponent<Text>();

        if (killedText != null) {
            killedText.text = string.Format("{0} was killed by {1}", message.dead, message.killer);
            StartCoroutine(this.showKilledText(5.0f, 0.0f, killedText));
        }
    }

    // Update the health/damage screen overlay.
    private void updateDamageScreen(float damageAmount) {
        if (!this.isLocalPlayer || (this._damageImage == null) || (((this._damageImage.color.a >= 1.0f) || this._isDead) && GameInfo.gamemode == "Battleroyale"))
            return;

        this._currentHealth -= damageAmount;
        this._isDead         = (this._currentHealth < 1.0f);

        float alpha             = (1.0f - this._currentHealth / MAX_HEALTH);
        this._damageImage.color = new Color(this._damageImage.color.r, this._damageImage.color.g, this._damageImage.color.b, alpha);


    }
}
