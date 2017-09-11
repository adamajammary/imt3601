using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Water : MonoBehaviour {

    public Image waterScreenEffect;
    private float _waterForce = 10.0f;

    void OnTriggerStay(Collider other) { 
        Debug.Log("Something in water!!");
        if (other.tag == "Player" || other.tag == "Enemy") {
            Debug.Log("Bunny in water!");
            other.GetComponent<CharacterController>().Move(Vector3.up * this._waterForce * Time.deltaTime);
        }

        if (other.tag == "Player")
            waterScreenEffect.enabled = true; 
    }

    void OnTriggerExit(Collider other) {
        if (other.tag == "Player")
            waterScreenEffect.enabled = false;
    }
}

