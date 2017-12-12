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
    private const float         _syncPerNPC = 0.7f;

    //The NPCs will sync at different rates, but the total syncs per second for all NPCs is 
    // 1 sync per second per npc.
    //Sync factor gives npcs a sync priority, so NPCs with a higher sync factor
    // will get more of the total syncs per second then npcs with a low syncfactor.
    //NPCs that are closer to players get a higher sync factor.

    public static int           syncCount = 0;
    public static float         _totalSyncFactor = 10000; 
    private static int          _npcCount = 0;
    private float               _oldSyncFactor = 0;
    private float               _syncFactor = 0;
    private float               _syncRate = _syncPerNPC; //How many times to sync per second   
    private float               _syncTimer = 0;
    private Vector3             _moveDir;
    private Vector3             _goal; //Used by the brain, need it here for syncing
    private CharacterController _cc;
    private GameObject          _blood;
    private GameObject          _fire;
    private float               _yVel;
    private int                 _syncFrame = 0;     //Which frame should this npc do syncing? (Used to distribute the load)
    private int                 _syncCounter = 0;   //Counting frames

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
        _npcCount++;

        if (this.isServer) {
            this._syncTimer = 0;
            this.syncClients();
        }
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
            this._syncCounter++;
            this._syncTimer += Time.deltaTime;
            if (this._syncTimer > (1 / this._syncRate)) {
                this.syncClients();
                this._syncTimer = 0;
            }
            //if ((this._syncCounter + this._syncFrame) % 5 == 0) {
            //    calcSyncRate();               
            //}
        }
    }

    public void setGoal(Vector3 goal) {
        this._goal = goal;
    }

    public Vector3 getGoal() {
        return this._goal;
    }

    public float getSyncRate() {
        return this._syncRate;
    }

    public float getSyncFactor() {
        return this._syncFactor;
    }

    public void burn() {
        CmdBurn(this.transform.position);
        die();
    }

    public void spawn(Vector3 pos, Vector3 dir, int frame = 0) {
        this._syncFrame = frame;
        IsDead = false;
        pos.y += 20;
        this.transform.position = pos;
        this._moveDir = dir;
        this.syncClients();
    }

    private void calcSyncRate() {
        this._oldSyncFactor = this._syncFactor;
        this._syncFactor = 400.0f / closestPlayer();        
        this._syncRate = _syncFactor * ((_npcCount  * _syncPerNPC) / _totalSyncFactor);
    }

    private void updateTotalSyncFactor() {
        _totalSyncFactor += this._syncFactor - this._oldSyncFactor;
    }

    private float closestPlayer() {
        float bestDist = 999999;
        float dist = 999999;
        var players = NPCWorldView.players;
        foreach (var player in players.Values) {
            dist = Vector3.Distance(transform.position, player.getPos());
            if (dist < bestDist) bestDist = dist; 
        }
        return (bestDist > 1) ? bestDist : 1;
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
        syncCount++;
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

    private void OnDestroy() {
        _npcCount--;
    }
}