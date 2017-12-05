using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// All of the logic for the NPC will be handeled by NPCThread
public class NPC : NetworkBehaviour {
    public string type = "not set";

    [SyncVar(hook = "updateMasterPos")]
    private Vector3 _masterPos;
    [SyncVar(hook = "updateMasterDir")]
    private Vector3 _masterDir;
    [SyncVar(hook = "updateGoal")]
    private Vector3 _masterGoal;

    private const float         _gravity = -12;
    private const float         _syncRate = 1; //How many times to sync per second

    private float               _syncTimer;
    private Vector3             _moveDir;
    private Vector3             _goal; //Used by the brain, need it here for syncing
    private CharacterController _cc;
    private GameObject          _blood;
    private GameObject          _fire;
    private float               _yVel;

    private Animator _ani;

    public bool IsDead; // Due to network delay.

    // Use this for initialization
    void Start() {
        this._cc    = GetComponent<CharacterController>();
        this._blood = Resources.Load<GameObject>("Prefabs/Blood");
        this._fire = Resources.Load<GameObject>("Prefabs/Fire");
        this._yVel  = 0;
        this._ani   = GetComponentInChildren<Animator>();

        this.IsDead = false;

        if (this.isServer) { this._syncTimer = 0; this.syncClients(); }
    }

    // Update is called once per frame
    void Update() {
        //fall
        this._yVel += Time.deltaTime * _gravity;
        if (this._cc.isGrounded)
            _yVel = 0;

        //move
        this._cc.Move(_moveDir * Time.deltaTime + new Vector3(0, this._yVel, 0));
        this.transform.LookAt(transform.position + this._moveDir);
        this._ani.SetFloat("movespeed", this._moveDir.magnitude * 4);

        //sync clients
        if (this.isServer) {
            this._syncTimer += Time.deltaTime;
            if (this._syncTimer > _syncRate) {
                this.syncClients();
                this._syncTimer = 0;
            }
        }
    }

    public void setGoal(Vector3 goal) {
        this._goal = goal;
    }

    public Vector3 getGoal() {
        return this._goal;
    }

    public void burn() {
        CmdBurn(this.transform.position);
        die();
    }

    public void spawn(Vector3 pos, Vector3 dir) {
        IsDead = false;
        pos.y += 20;
        this.transform.position = pos;
        this._moveDir = dir;
        this.syncClients();
    }

    private void updateMasterPos(Vector3 masterPos) {
        transform.position = masterPos;
    }

    private void updateMasterDir(Vector3 masterDir) {
        this._moveDir = masterDir;
    }

    private void updateGoal(Vector3 masterGoal) {
        this._goal = masterGoal;
    }

    public void update(Vector3 moveDir, Vector3 goal) {
        this._moveDir = moveDir;
        this._goal = goal;
    }

    private void syncClients() {
        this._masterPos = transform.position;
        this._masterDir = this._moveDir;
        this._masterGoal = this._goal;
    }

    public void die() {
        if (GameInfo.gamemode == "Battleroyale")
            NetworkServer.Destroy(this.gameObject);
        if (GameInfo.gamemode == "Deathmatch") {
            this.gameObject.SetActive(false);
        }
    }

    [Command]
    public void CmdBurn(Vector3 pos) {
        GameObject fire = Instantiate(this._fire);
        fire.transform.position = pos;
        NetworkServer.Spawn(fire);
        Destroy(fire, 10.0f);
    }
}