using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Water : MonoBehaviour {
    public Image waterScreenEffect;
    private float _waterForceStrength = 12.0f;
    private float _waterSurfaceHeight;

    private const float _noiseSpeed = 1.25f;
    private float _noiseSeed;
    private Material _shader;

    private void Start() {
        this._noiseSeed = Random.Range(0, 9999);
        this._shader = GetComponent<Renderer>().material;

        this._waterSurfaceHeight = transform.position.y;

        Material mat = GetComponent<Renderer>().material;
        //mat.SetInt("_ZWrite", 1);
        mat.renderQueue = 3000;
    }

    private void Update() {
        this._noiseSeed += _noiseSpeed * Time.deltaTime;
        this._shader.SetFloat("_NoiseSeed", this._noiseSeed);
    }

    void OnTriggerStay(Collider other) { 
        if (other.tag == "Player" || other.tag == "Enemy") {
            float waterForce = (this._waterSurfaceHeight - other.transform.position.y + 0.5f) * this._waterForceStrength;
            other.GetComponent<PlayerEffects>().onWaterStay(waterForce);
        }

        if (other.tag == "Player")
            waterScreenEffect.enabled = true; 
    }

    void OnTriggerExit(Collider other) {
        if (other.tag == "Player")
            waterScreenEffect.enabled = false;
    }
}

