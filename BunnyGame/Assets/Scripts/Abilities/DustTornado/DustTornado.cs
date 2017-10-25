using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//Networking behaviour of this projectile:
//If the tornado hits someone on the attackers client, then that will count as a hit.
public class DustTornado : NetworkBehaviour {
    public Transform tornadoParticles;

    private const float _lifeSpan = 10;
    private const float _AOE = 75;
    private const float _spinLock = 10;
    private const float _speed = 10;
    private float timer = 0;
    private GameObject _player; //The local player in this game instance
    private GameObject[] _enemies;
    private bool[] _trapped;
    [SyncVar] private GameObject _owner;  //The player who actually spawned the tornado
    [SyncVar] private Vector3 _dir;

    private void Start() {
        this._player = GameObject.FindGameObjectWithTag("Player");
        this._enemies = GameObject.FindGameObjectsWithTag("Enemy");
        this._trapped = new bool[this._enemies.Length];
        for (int i = 0; i < this._trapped.Length; i++) this._trapped[i] = false;
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
    

    public void shoot(Vector3 pos, Vector3 dir, GameObject owner) {
        transform.position = pos;
        this._dir = dir;
        this._owner = owner;
    }

    private void updateEnemy(int i) {
        if (this._enemies[i] != null) {
            Debug.Log("WORK FUCKIT");
            float dist = Vector3.Distance(transform.position, this._enemies[i].transform.position);
            if (dist < _AOE) {
                Debug.Log("HOOK EM");
                if (dist <= _spinLock && !this._trapped[i]) {
                    this._trapped[i] = true;
                    this._enemies[i].GetComponent<PlayerEffects>().CmdSetCC(false);
                }
                if (this._trapped[i])
                    this._enemies[i].GetComponent<PlayerEffects>().CmdTornadoSpin(this.transform.position);
                else
                    this._enemies[i].GetComponent<PlayerEffects>().CmdTornadoPullIn(this.transform.position);
            }
        }
    }

    //Cleanup is important, so Destroy(this, time) wont guarantee that this is the client that does the cleanup with OnDestroy
    private void calcDeath() {
        timer += Time.deltaTime;
        if (timer > _lifeSpan) {
            cleanup();
            Destroy(this.gameObject);
        }
    }

    private void cleanup() {
        for (int i = 0; i < this._trapped.Length; i++) {
            if (this._trapped[i]) {
                this._enemies[i].GetComponent<PlayerEffects>().CmdSetCC(false);
            }
        }
    }
}
