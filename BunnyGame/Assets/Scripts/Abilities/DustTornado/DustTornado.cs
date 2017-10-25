using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//Networking behaviour of this projectile:
//If the tornado hits someone on the attackers client, then that will count as a hit.
//It basically implements the reversed logic
public class DustTornado : NetworkBehaviour {
    private class EnemyData {
        public bool trapped;
        public float yOffset;

        public EnemyData() {
            trapped = false;
            yOffset = 0;
        }
    }

    public Transform tornadoParticles;

    private const float _lifeSpan = 6;
    private const float _AOE = 75;
    private const float _spinLock = 10;
    private const float _speed = 10;
    private float timer = 0;
    private GameObject _player; //The local player in this game instance
    private GameObject[] _enemies;
    private EnemyData[] _ed;
    [SyncVar] private GameObject _owner;  //The player who actually spawned the tornado
    [SyncVar] private Vector3 _dir;

    private void Start() {
        this._player = GameObject.FindGameObjectWithTag("Player");
        this._enemies = GameObject.FindGameObjectsWithTag("Enemy");
        this._ed = new EnemyData[this._enemies.Length];
        for (int i = 0; i < this._ed.Length; i++) this._ed[i] = new EnemyData();
    }

    void Update() {
        tornadoParticles.Rotate(Vector3.up, 200 * Time.deltaTime, Space.World);
        Vector3 vel = this._dir * _speed;
        vel.y = GetComponent<Rigidbody>().velocity.y;
        GetComponent<Rigidbody>().velocity = vel;

        if (this._player == this._owner) {
            calcDeath();
            for (int i = 0; i < this._enemies.Length; i++) {
                updateEnemy(i);               
            }
        }
    }
    
    public void kill() {
        this.timer = _lifeSpan;
    }

    public void shoot(Vector3 pos, Vector3 dir, GameObject owner) {
        transform.position = pos;
        this._dir = dir;
        this._owner = owner;
    }

    private void updateEnemy(int i) {
        if (this._enemies[i] != null) {
            float dist = Vector3.Distance(transform.position, this._enemies[i].transform.position);
            if (dist < _AOE) {
                if (dist <= _spinLock && !this._ed[i].trapped) {
                    this._ed[i].trapped = true;
                    CmdSetCC(this._enemies[i], true);
                }
                if (this._ed[i].trapped && true) {
                    CmdTornadoSpin(this._enemies[i], this._ed[i].yOffset);
                    if (this._ed[i].yOffset < 20)
                        this._ed[i].yOffset += 6f * Time.deltaTime;
                } else
                    CmdTornadoPullIn(this._enemies[i]);
            }
        }
    }

    [Command]
    public void CmdTornadoPullIn(GameObject player) {
        TargetTornadoPullIn(player.GetComponent<PlayerInformation>().connectionToClient, player);
    }

    [TargetRpc]
    private void TargetTornadoPullIn(NetworkConnection target, GameObject player) {
        Vector3 dir = this.transform.position - player.transform.position;
        player.GetComponent<CharacterController>().Move(dir.normalized * 12 * Time.deltaTime);
    }

    [Command]
    public void CmdTornadoSpin(GameObject player, float offset) {
        TargetTornadoSpin(player.GetComponent<PlayerInformation>().connectionToClient, player, offset);
    }

    [TargetRpc]
    private void TargetTornadoSpin(NetworkConnection target, GameObject player, float offset) {
        Vector3 pos = this.transform.position + Vector3.up * offset;
        Vector3 dir = player.transform.position - pos;
        dir.y = 0; dir.Normalize();
        Vector3 spinDir = Quaternion.AngleAxis(180 * Time.deltaTime, Vector3.up) * dir;
        RaycastHit hit;
        Physics.Raycast(transform.position, spinDir, out hit);
        float len = (hit.distance < 10) ? hit.distance : 10;
        player.transform.position = pos + spinDir * len;
    }

    [Command]
    public void CmdSetCC(GameObject player, bool value) {
        TargetSetCC(player.GetComponent<PlayerInformation>().connectionToClient, player, value);
    }

    [TargetRpc]
    private void TargetSetCC(NetworkConnection target, GameObject player, bool value) {
        player.GetComponent<PlayerController>().setCC(value);
    }

    [Command]
    public void CmdDestroy() {
        Destroy(this.gameObject);
    }

    //Cleanup is important, so Destroy(this, time) wont guarantee that this is the client that does the cleanup with OnDestroy
    private void calcDeath() {
        timer += Time.deltaTime;
        if (timer > _lifeSpan) {
            cleanup();
            CmdDestroy();
        }
    }

    private void cleanup() {
        for (int i = 0; i < this._ed.Length; i++) {
            if (this._ed[i].trapped) {
                CmdSetCC(this._enemies[i], false);
            }
        }
    }
}
