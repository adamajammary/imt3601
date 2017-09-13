using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoxController : MonoBehaviour {

	// Use this for initialization
	void Start () {
        PlayerController playerController = GetComponent<PlayerController>();
        playerController.runSpeed = 15;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
