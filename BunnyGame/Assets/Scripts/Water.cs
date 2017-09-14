using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Water : MonoBehaviour {

    public Image waterScreenEffect;
    private float _waterForceStrength = 12.0f;
    private float _waterSurfaceHeight;

    private void Start() {
        this._waterSurfaceHeight = transform.position.y + transform.localScale.y / 2;
    }

    void OnTriggerStay(Collider other) { 
        if (other.tag == "Player" || other.tag == "Enemy") {
            float waterForce = (this._waterSurfaceHeight - other.transform.position.y + 0.5f) * this._waterForceStrength;
            Debug.Log(waterForce);
            other.GetComponent<PlayerController>().onWaterStay(waterForce);
        }

        if (other.tag == "Player")
            waterScreenEffect.enabled = true; 
    }

    void OnTriggerExit(Collider other) {
        if (other.tag == "Player")
            waterScreenEffect.enabled = false;
    }
}

