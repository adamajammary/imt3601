//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

//public class BunnyPoop : MonoBehaviour {
public class BunnyPoop : NetworkBehaviour {
    public GameObject owner; //The gameobject which owns this 

    private int       _timeToLive = 3;
    private float     _timeAlive = 0;
    private float     _speed = 30.0f;
    private float     _antiGravity = 5.0f;
    private Rigidbody _rb;
    private int       _damage = 10;
    private int       _connectionID = -1;

    private void Awake() {
        this._rb = this.GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        // to reduce bullet drop
        this._rb.AddForce(Vector3.up * this._antiGravity);

        this._timeAlive += Time.deltaTime;
        if (this._timeAlive > this._timeToLive)
            Destroy(this.gameObject);
    }

    public void shoot(Vector3 dir, Vector3 pos, Vector3 startVel) {
        this.transform.position = pos;
        this._rb.velocity = dir * _speed + startVel;          
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.tag != "Player")
            Destroy(this.gameObject);
    }

    // Increase the player kill count.
    public void AddKill() {
        //print("AddKill::client: " + NetworkClient.allClients[0]);
        //print("AddKill::id: " + _connectionID);

        //if (this.isServer && this.isClient)
        //    this.addKill2();
        //else if (this.isServer)
        //    this.RpcAddKill();
        //else if (this.isClient)
        //    this.CmdAddKill();

        if (NetworkClient.allClients[0] != null)
            NetworkClient.allClients[0].Send((short)NetworkMessageType.MSG_PLAYERKILLED, new IntegerMessage(this._connectionID));
    }

    //private void addKill2() {
    //    print("addKill2: " + this._connectionID);

    //    NetworkClient client = NetworkClient.allClients[0];

    //    if (client != null)
    //        client.Send((short)NetworkMessageType.MSG_PLAYERKILLED, new IntegerMessage(this._connectionID));
    //}

    //[ClientRpc]
    //private void RpcAddKill() {
    //    this.addKill2();
    //}

    //[Command]
    //private void CmdAddKill() {
    //    this.RpcAddKill();
    //}

    public int GetDamage() {
        return this._damage;
    }

    // Assign the player connection ID to the projectile.
    public void SetConnectionID(int id) {
        this._connectionID = id;

        //print("SetConnectionID: " + this._connectionID);
    }
}
