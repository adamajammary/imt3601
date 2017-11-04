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

    // NB! Handled in PlayerAttack.cs
    /*//public void kill(GameObject killer, int id) {
    public void kill(GameObject killer) {
        if (this.IsDead) return;
        this.IsDead = true;
        PlayerHealth healthScript = killer.GetComponent<PlayerHealth>();
        PlayerEffects pe = killer.GetComponent<PlayerEffects>();
        this.CmdBloodParticle(this.transform.position);
        
        switch (this.type) {
            case "whale":
                pe.CmdAddToughness(0.05f);
                break;
            case "cat":
                pe.CmdAddDamage(0.05f);
                break;
            case "dog":
                pe.CmdAddSpeed(0.05f);
                break;
            case "eagle":
                pe.CmdAddJump(0.05f);
                break;
            case "chicken":
                //healthScript.TakeDamage(-10, id);
                healthScript.Heal(10.0f);
                break;
        }

        CmdDestroy();
    }*/

    public void burn() {
        CmdBurn(this.transform.position);
        Destroy(this.gameObject);
    }

    public void spawn(Vector3 pos, Vector3 dir) {
        this.transform.position = pos;
        this._moveDir = dir;
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

    // NB! Handled in PlayerAttack.cs
    /*private void OnCollisionEnter(Collision other) {
        if (other.gameObject.tag == "projectile") {
            BunnyPoop poopScript = other.gameObject.GetComponent<BunnyPoop>();
            //PlayerInformation otherInfo = poopScript.owner.GetComponent<PlayerInformation>();
            //kill(poopScript.owner, otherInfo.ConnectionID);
            kill(poopScript.owner);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.gameObject.tag == "foxbite"))
        {
            //PlayerInformation otherInfo = other.GetComponentInParent<PlayerInformation>();
            //kill(other.transform.parent.gameObject, otherInfo.ConnectionID);
            kill(other.transform.parent.gameObject);
        }
        else if ((other.gameObject.tag == "mooseAttack"))
        {
            //PlayerInformation otherInfo = other.GetComponentInParent<PlayerInformation>();
            //kill(other.transform.parent.gameObject, otherInfo.ConnectionID);
            kill(other.transform.parent.gameObject);
        }
    }

    [Command]
    private void CmdDestroy() {
        NetworkServer.Destroy(gameObject);
    }

    [Command]
    public void CmdBloodParticle(Vector3 hitPosition) {
        GameObject blood = Instantiate(this._blood);

        blood.transform.position = hitPosition;

        NetworkServer.Spawn(blood);
        Destroy(blood, 5.0f);
    }*/

    [Command]
    public void CmdBurn(Vector3 pos) {
        GameObject fire = Instantiate(this._fire);
        fire.transform.position = pos;
        NetworkServer.Spawn(fire);
        Destroy(fire, 10.0f);
    }
}