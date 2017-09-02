using UnityEngine;
using UnityEngine.Networking;

public class PlayerNetworkingHealth : NetworkBehaviour {
    [SerializeField] private bool          _destroyOnDeath = false;
    [SerializeField] private RectTransform _healthBar;

    private const int              _maxHealth = 100;
    private NetworkStartPosition[] _spawnPositions;

    [SyncVar(hook = "OnChangeHealth")] private int _currentHealth = _maxHealth;

    // Use this for initialization
    private void Start() {
        if (this.isLocalPlayer) {
            this._spawnPositions = FindObjectsOfType<NetworkStartPosition>();
        }
    }

    // Update is called once per frame
    private void Update() {
    }

    // Update the health bar canvas on the clients whenever the health value changes on the server
    private void OnChangeHealth(int health) {
        this._healthBar.sizeDelta = new Vector2(health * 2, this._healthBar.sizeDelta.y);
    }

    [ClientRpc]
    private void RpcRespawn() {
        if (!this.isLocalPlayer) { return; }

        Vector3 spawnPosition = Vector3.zero;

        if ((this._spawnPositions != null) && (this._spawnPositions.Length > 0)) {
            spawnPosition = this._spawnPositions[Random.Range(0, this._spawnPositions.Length)].transform.position;
        }

        this.transform.position = spawnPosition;
    }

    // Take damage when hit, and respawn the player
    public void TakeDamage(int amount) {
        if (!this.isServer) { return; }

        this._currentHealth -= amount;
        
        if (this._currentHealth <= 0) {
            if (this._destroyOnDeath) {
                Destroy(this.gameObject);
            } else {
                this._currentHealth = _maxHealth;
                RpcRespawn();
            }
        }
    }
}
