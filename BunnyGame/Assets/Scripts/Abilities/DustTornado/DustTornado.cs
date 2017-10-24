using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustTornado : MonoBehaviour {

    public Transform tornadoParticles;

    private const float _speed = 10;
    private Vector3 _dir;

    void Update() {
        tornadoParticles.Rotate(Vector3.up, 200 * Time.deltaTime, Space.World);
        GetComponent<Rigidbody>().AddForce(this._dir * _speed);
    }   

    public void shoot(Vector3 pos, Vector3 dir) {
        transform.position = pos;
        this._dir = dir;
    }
}
