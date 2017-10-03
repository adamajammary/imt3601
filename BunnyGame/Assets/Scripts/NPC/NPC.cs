using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// All of the logic for the NPC will be handeled by NPCThread
public class NPC : NetworkBehaviour {
    [SyncVar(hook = "spawnPos")]
    private Vector3 _spawnPos;
    [SyncVar(hook = "spawnRot")]
    private Quaternion _spawnRot;
    private Vector3 _moveDir;
    private CharacterController _cc;
    private GameObject _blood;
    private const float _gravity = -12;
    private float _yVel;
    // Use this for initialization
    void Start () {
        this._cc = GetComponent<CharacterController>();
        this._blood = Resources.Load<GameObject>("Prefabs/Blood");
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

    public void spawnPos(Vector3 _spawnPos) {
        transform.position = _spawnPos;
    }

    public void spawnRot(Quaternion _spawnRot) {
        transform.rotation = _spawnRot;
    }

    public void setSpawnPos(Vector3 spawnPos) {
        this._spawnPos = spawnPos;
        this._moveDir = this.transform.forward;
    }

    public void setSpawnRot(Quaternion spawnRot) {
        this._spawnRot = spawnRot;
    }

    public void setMoveDir(Vector3 moveDir) {
        this._moveDir = moveDir;
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.tag == "projectile") {
            CmdBloodParticle(other.gameObject.transform.position);
            Destroy(other.gameObject);
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (!this.isServer)
            return;
        //if (other.gameObject.tag == "foxbite" && other.transform.parent != transform) {
        if ((other.gameObject.tag == "foxbite")) {
            CmdBloodParticle(other.GetComponentInParent<FoxController>().biteImpact());
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
