using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// All of the logic for the NPC will be handeled by NPCThread
public class NPC : NetworkBehaviour {
    [SyncVar(hook = "spawn")]
    private Vector3 _spawnPos;
    [SyncVar]
    private Vector3 _moveDir;
    private CharacterController _cc;
    private const float _gravity = -12;
    private float _yVel;
    // Use this for initialization
    void Start () {
        this._cc = GetComponent<CharacterController>();
        this._yVel = 0;
	}
	
	// Update is called once per frame
	void Update () {
        this._yVel += Time.deltaTime * _gravity;
        if (this._cc.isGrounded)
            _yVel = 0;
        this._cc.Move(_moveDir * Time.deltaTime + new Vector3(0, this._yVel, 0));
        this.transform.LookAt(transform.position + this._moveDir);
	}

    public void spawn(Vector3 _spawnPos) {
        transform.position = _spawnPos;
    }

    public void setSpawnPos(Vector3 spawnPos) {
        this._spawnPos = spawnPos;
    }

    public void setMoveDir(Vector3 moveDir) {
        this._moveDir = moveDir;
    }
}
