using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BunnyPoop : MonoBehaviour {

    private int _timeToLive = 3;
    private float _timeAlive = 0;
    private float _speed = 20.0f;
	
	// Update is called once per frame
	void Update () {
        this._timeAlive += Time.deltaTime;
        if (this._timeAlive > this._timeToLive)
            Destroy(this.gameObject);
    }

    public void shoot(Vector3 dir, Vector3 pos) {
        Rigidbody rb = this.GetComponent<Rigidbody>();
        this.transform.position = pos;
        rb.velocity = dir * _speed;          
    }

    private void OnCollisionEnter() {
        Destroy(this.gameObject);
    }
}
