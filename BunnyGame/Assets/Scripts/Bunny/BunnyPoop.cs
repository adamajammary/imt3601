using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BunnyPoop : NetworkBehaviour {

    private int _timeToLive;
    private float _timeAlive;
    private float _speed;

	// Use this for initialization
	void Start () {
        this._timeToLive = 3;
        this._timeAlive = 0;
        this._speed = 20.0f;
	}
	
	// Update is called once per frame
	void Update () {
        this._timeAlive += Time.deltaTime;

        if (this._timeAlive > this._timeToLive)
            this.destroy();
	}

    public void shoot(Vector3 dir, Vector3 pos) {
        Rigidbody rb = this.GetComponent<Rigidbody>();
        this.transform.position = pos;
        rb.velocity = dir * _speed;
        this._timeAlive = 0;
    }

    private void destroy() {
        this.gameObject.SetActive(false);
    }

    private void OnCollisionEnter() {
        this.destroy();
    }
}
