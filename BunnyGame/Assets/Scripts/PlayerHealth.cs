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
    private Image     _spectateImage;
    private Text      _spectateText;
    private bool      _showDeathScreen;

    [SyncVar(hook = "OnChangeHealth")]
    private int _currentHealth = MAX_HEALTH;

    void Start() {
        if (!this.isLocalPlayer) { return; }

        this._damageImage     = GameObject.Find("Canvas/BloodSplatterOverlay").GetComponent<Image>();
        this._isDead          = false;
        this._isDeadText      = GameObject.Find("Canvas/PlayerIsDeadText").GetComponent<Text>();
        this._spectateImage   = GameObject.Find("Canvas/SpectateButton").GetComponent<Image>();
        this._spectateText    = GameObject.Find("Canvas/SpectateButton/SpectateButtonText").GetComponent<Text>();
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
        return this._isDead;
    }

    // Update the health/damage on the client whenever the health value changes on the server.
    private void OnChangeHealth(int health) {
        if (!this.isLocalPlayer) { return; }

        this.updateDamageScreen((float)health);
        
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
    }

    // Take damage when hit, and respawn the player if dead.
    public void TakeDamage(int amount) {
        if (!this.isServer || this._damageImmune) { return; }

        this._currentHealth -= amount;
        
        if (!this._isDead && (this._currentHealth <= 0)) {
            this.RpcDie();
        }
    }

    public void TakeDamage(float amount) {
        this.TakeDamage((int)amount);
    }

    // Update the health/damage screen overlay.
    private void updateDamageScreen(float health) {
        if ((this._damageImage == null) || (this._damageImage.color.a >= 1.0f)) { return; }

        float alpha             = (1.0f - (float)health / (float)MAX_HEALTH);
        this._damageImage.color = new Color(this._damageImage.color.r, this._damageImage.color.g, this._damageImage.color.b, alpha);
    }

    // Respawn the player in a new random position.
    [ClientRpc]
    private void RpcDie() {
        Debug.Log("TODO: DISABLE ATTACK ABILITIES");
        this._isDead = true;
        this.gameObject.transform.GetChild(1).gameObject.SetActive(false);
    }
}
