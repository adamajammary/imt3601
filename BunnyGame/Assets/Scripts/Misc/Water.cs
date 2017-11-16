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


        //if (other.tag == GameObject.Find("Main Camera").GetComponent<ThirdPersonCamera>().getTargetTag())
        //    waterScreenEffect.enabled = true;


        if (other.tag == "Player") {
            other.GetComponent<PlayerController>().inWater = true;
            waterScreenEffect.enabled = true;
        }
        else if (other.tag == "bunnycamera") {
            if (other.transform.parent.tag == "Player") {
                other.transform.parent.GetComponent<PlayerController>().inWater = true;
                waterScreenEffect.enabled = true;
            }
        }

    }

    void OnTriggerExit(Collider other) {
 //       if (other.tag == GameObject.Find("Main Camera").GetComponent<ThirdPersonCamera>().getTargetTag())
 //           waterScreenEffect.enabled = false;


        if (other.tag == "Player") {
            other.GetComponent<PlayerController>().inWater = false;
            waterScreenEffect.enabled = false;
        }
        else if (other.tag == "bunnycamera") {
            if(other.transform.parent.tag == "Player") {
                other.transform.parent.GetComponent<PlayerController>().inWater = false;
                waterScreenEffect.enabled = false;
            }
        }
    }
}

