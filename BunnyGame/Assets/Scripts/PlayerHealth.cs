using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour {

    private const int MAX_HEALTH = 100;
    private bool _damageImmune = true;

    [SyncVar(hook = "OnChangeHealth")]
    private int _currentHealth = MAX_HEALTH;

    void Start() {
        StartCoroutine(damageImmune(5)); // Make player immune from damage for the first 5 seconds after spawning
    }

    // Update the health/damage screen overlay on the client whenever the health value changes on the server
    private void OnChangeHealth(int health) {
        if (!this.isLocalPlayer) { return; }

        Image image = GameObject.Find("Canvas/BloodSplatterOverlay").GetComponent<Image>();
        if (!image) { return; }

        float alpha = (1.0f - (float)health / (float)MAX_HEALTH);
        image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
    }

    // Take damage when hit, and respawn the player if dead.
    public void TakeDamage(int amount) {
        if (!this.isServer || _damageImmune) { return; }

        this._currentHealth -= amount;
        
        if (this._currentHealth <= 0) {
            this._currentHealth = MAX_HEALTH;
            this.RpcSpawn();
        }
    }
    public void TakeDamage(float amount) {
        TakeDamage((int)amount);
    }

    // Make the player immune from damage for <seconds>
    private IEnumerator damageImmune(float seconds) {
        _damageImmune = true;
        yield return new WaitForSeconds(seconds);
        _damageImmune = false;
    }


    // Respawn the player in a new random position.
    [ClientRpc]
    private void RpcSpawn() {
        transform.position = new Vector3(Random.Range(-40, 40), 10, Random.Range(-40, 40));
    }
}
