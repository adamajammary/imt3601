using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// All of the logic for the NPC will be handeled by NPCThread
public class NPC : NetworkBehaviour {
    [SyncVar(hook = "updateMasterPos")]
    private Vector3 _masterPos;
    [SyncVar(hook = "updateMasterDir")]
    private Vector3 _masterDir;

    private const float _gravity = -12;
    private const float _syncRate = 1; //How many times to sync per second

    private float _syncTimer;
    private Vector3 _moveDir;
    private CharacterController _cc;
    private GameObject _blood;
    private float _yVel;
    // Use this for initialization
    void Start () {
        this._cc = GetComponent<CharacterController>();
        this._blood = Resources.Load<GameObject>("Prefabs/Blood");
        this._yVel = 0;

        if (this.isServer) this._syncTimer = 0;
	}
	
	// Update is called once per frame
	void Update () {
        this._yVel += Time.deltaTime * _gravity;
        if (this._cc.isGrounded)
            _yVel = 0;
        this._cc.Move(_moveDir * Time.deltaTime + new Vector3(0, this._yVel, 0));
        this.transform.LookAt(transform.position + this._moveDir);

        if (this.isServer) {
            this._syncTimer += Time.deltaTime;
            if (this._syncTimer > _syncRate) {
                this.syncClients();
                this._syncTimer = 0;
            }
        }
	}

    public void updateMasterPos(Vector3 masterPos) {
        transform.position = masterPos;
    }

    public void updateMasterDir(Vector3 masterDir) {
        this._moveDir = masterDir;
    }

    public void setMoveDir(Vector3 moveDir) {
        this._moveDir = moveDir;
    }

    public void spawn(Vector3 pos, Quaternion rot) {
        this.transform.position = pos;
        this.transform.rotation = rot;
        this._moveDir = transform.forward;
    }

    public void syncClients() {
        this._masterPos = transform.position;
        this._masterDir = this._moveDir;
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.tag == "projectile") {
            BunnyPoop poopScript = other.gameObject.GetComponent<BunnyPoop>();
            PlayerHealth healthScript = poopScript.owner.GetComponent<PlayerHealth>();

            this.CmdBloodParticle(other.gameObject.transform.position);
            healthScript.TakeDamage(-10, poopScript.ConnectionID);

            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if ((other.gameObject.tag == "foxbite")) {
            PlayerHealth healthScript = other.transform.parent.GetComponent<PlayerHealth>();
            FoxController foxScript = other.GetComponentInParent<FoxController>();
  
            this.CmdBloodParticle(foxScript.biteImpact());
            healthScript.TakeDamage(-10, foxScript.ConnectionID);
            Destroy(this.gameObject);
        } 
    }

    [Command]
    public void CmdBloodParticle(Vector3 hitPosition) {
        GameObject blood = Instantiate(this._blood);

        blood.transform.position = hitPosition;

        NetworkServer.Spawn(blood);
        Destroy(blood, 5.0f);
    }
}
