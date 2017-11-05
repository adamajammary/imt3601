using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Water : MonoBehaviour {
    public Image waterScreenEffect;
    private float _waterForceStrength = 12.0f;
    private float _waterSurfaceHeight;

    private void Start() {       
        this._waterSurfaceHeight = transform.position.y;

        Material mat = GetComponentInChildren<Renderer>().material;
        //mat.SetInt("_ZWrite", 1);
        mat.renderQueue = 3000;
    }

    void OnTriggerStay(Collider other) {
        if (other.tag == "PoopGrenade") return;

        if (other.tag == "Player") {
            float waterForce = (this._waterSurfaceHeight - other.transform.position.y + 0.5f) * this._waterForceStrength;
            other.GetComponent<PlayerEffects>().onWaterStay(waterForce);
        }
        else if (other.tag == "Enemy") {
            float waterForce = (this._waterSurfaceHeight - other.transform.position.y + 0.5f) * this._waterForceStrength;
            other.GetComponent<PlayerEffects>().onWaterStay(waterForce);
        }
        else if(other.tag == "bunnycamera") {
            float waterForce = (this._waterSurfaceHeight - other.transform.parent.position.y + 0.5f) * this._waterForceStrength;
            other.transform.parent.GetComponent<PlayerEffects>().onWaterStay(waterForce);
        }

        if (other.tag == GameObject.Find("Main Camera").GetComponent<ThirdPersonCamera>().getTargetTag())
            waterScreenEffect.enabled = true;

        //Debug.Log(other.name + " :: " + GameObject.Find("Main Camera").GetComponent<ThirdPersonCamera>().getTargetTag());
    }

    void OnTriggerExit(Collider other) {
        if (other.tag == GameObject.Find("Main Camera").GetComponent<ThirdPersonCamera>().getTargetTag())
            waterScreenEffect.enabled = false;
    }
}

