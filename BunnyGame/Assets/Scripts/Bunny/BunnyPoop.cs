using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BunnyPoop : MonoBehaviour {

    private int         _timeToLive = 3;
    private float       _timeAlive = 0;
    private float       _speed = 20.0f;
    private float       _antiGravity = 1.0f;
    private Rigidbody   _rb;
    //private bool        _init;
    private int         _damage = 10;

    private void Awake() {
        //this._init = false;
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

    public int GetDamage() {
        return this._damage;
    }
}
