using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 pos;
		if (Input.GetKey(KeyCode.W)) {
            pos = transform.position;
            pos.y += 1;
            transform.position = pos; 
        }
        else if (Input.GetKey(KeyCode.S)) {
            pos = transform.position;
            pos.y += -1;
            transform.position = pos;
        }

    }
}
