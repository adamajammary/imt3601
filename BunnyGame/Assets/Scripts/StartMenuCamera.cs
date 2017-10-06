using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMenuCamera : MonoBehaviour {

    GameObject camera;
	// Use this for initialization
	void Start () {
        camera = gameObject;
	}
	
	// Update is called once per frame
	void Update () {
        camera.transform.LookAt(Vector3.zero);
        camera.transform.Translate(Vector3.right * Time.deltaTime);  
	}
}
