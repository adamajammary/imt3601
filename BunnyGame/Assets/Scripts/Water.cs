using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Water : MonoBehaviour {

    public Image waterScreenEffect;
    private float _waterForceStrength = 10.0f;
    private float _waterSurfaceHeight;

    private void Start() {
        this._waterSurfaceHeight = transform.position.y + transform.localScale.y / 2;
    }

    void OnTriggerStay(Collider other) { 
        if (other.tag == "Player" || other.tag == "Enemy") {
            float waterForce = (this._waterSurfaceHeight - other.transform.position.y) * this._waterForceStrength;
            Debug.Log(waterForce);
            other.GetComponent<CharacterController>().Move(Vector3.up * waterForce * Time.deltaTime);
        }

        if (other.tag == "Player")
            waterScreenEffect.enabled = true; 
    }

    void OnTriggerExit(Collider other) {
        if (other.tag == "Player")
            waterScreenEffect.enabled = false;
    }
}

