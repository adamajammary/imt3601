using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour {

    private const int MAX_HEALTH = 100;
    private Image     _damageImage;
    private bool      _damageImmune;
    private bool      _isDead;
    private Text      _isDeadText;
    private Button    _spectateButton;
    private Image     _spectateImage;
    private Text      _spectateText;
    private bool      _showDeathScreen;

    [SyncVar(hook = "OnChangeHealth")]
    private int _currentHealth = MAX_HEALTH;

    void Start() {
        if (!this.isLocalPlayer) { return; }

        this._damageImage     = GameObject.Find("Canvas/BloodSplatterOverlay").GetComponent<Image>();
        this._isDeadText      = GameObject.Find("Canvas/PlayerIsDeadText").GetComponent<Text>();
        this._spectateButton  = GameObject.Find("Canvas/SpectateButton").GetComponent<Button>();
        this._spectateImage   = GameObject.Find("Canvas/SpectateButton").GetComponent<Image>();
        this._spectateText    = GameObject.Find("Canvas/SpectateButton/SpectateButtonText").GetComponent<Text>();
        this._isDead          = false;
        this._showDeathScreen = false;

        // Make player immune from damage for the first 5 seconds after spawning
        StartCoroutine(this.damageImmune(5.0f));
    }

    // Make the player immune from damage for <seconds>
    private IEnumerator damageImmune(float seconds) {
        this._damageImmune = true;
        yield return new WaitForSeconds(seconds);
        this._damageImmune = false;
    }

    public bool IsDead() {
        if (this.isClient)
            this.CmdIsDead();
        else if (this.isServer)
            this.RpcIsDead();

        return this._isDead;
    }
	
    [Command]
    private void CmdIsDead() {
        this.RpcIsDead();
    }

    [ClientRpc]
    private void RpcIsDead() {
        this._isDead = (this._currentHealth <= 0);
    }

    // Update the health/damage on the client whenever the health value changes on the server.
    private void OnChangeHealth(int health) {
        if (!this.isLocalPlayer) { return; }

        this.updateDamageScreen((float)health);

        this._isDead = (health <= 0);

        if (this._isDead && !this._showDeathScreen) {
			this.showDeathScreen();
        }
    }

    // Show the death screen.
    private void showDeathScreen() {
		if ((this._isDeadText == null) || (this._spectateImage == null) || (this._spectateText == null)) { return; }

        this._showDeathScreen     = true;
        this._isDeadText.color    = new Color(this._isDeadText.color.r,    this._isDeadText.color.g,    this._isDeadText.color.b,    1.0f);
        this._spectateImage.color = new Color(this._spectateImage.color.r, this._spectateImage.color.g, this._spectateImage.color.b, 1.0f);
        this._spectateText.color  = new Color(this._spectateText.color.r,  this._spectateText.color.g,  this._spectateText.color.b,  1.0f);

        this._spectateButton.onClick.AddListener(this.spectate); // TODO: see spectate method below
    }

    // Update the health/damage screen overlay.
    private void updateDamageScreen(float health) {
		if ((this._damageImage == null) || (this._damageImage.color.a >= 1.0f)) { return; }

        float alpha             = (1.0f - (float)health / (float)MAX_HEALTH);
        this._damageImage.color = new Color(this._damageImage.color.r, this._damageImage.color.g, this._damageImage.color.b, alpha);
    }

    // TODO: Implement spectating
    private void spectate() {
        Debug.Log("TODO: Implement spectating");
    }

    // Take damage when hit, and respawn the player if dead.
    public void TakeDamage(int amount) {
        if (!this.isLocalPlayer || this._damageImmune || this.IsDead()) { return; }

        if (this.isClient)
            this.CmdTakeDamage(amount);
        else if (this.isServer)
            this.RpcTakeDamage(amount);
    }

    [Command]
    private void CmdTakeDamage(int amount) {
        this.RpcTakeDamage(amount);
    }

    [ClientRpc]
    private void RpcTakeDamage(int amount) {
        this._currentHealth -= amount;
        this._isDead         = (this._currentHealth <= 0);

        if (this._isDead)
            this.Die();
    }

    // Respawn the player in a new random position.
    private void Die() {
        if (this.isClient)
            this.CmdDie();
        else if (this.isServer)
            this.RpcDie();
    }
	
    [Command]
    private void CmdDie() {
        this.RpcDie();
    }

    [ClientRpc]
    private void RpcDie() {
        this.gameObject.transform.GetChild(1).gameObject.SetActive(false);
    }
}
