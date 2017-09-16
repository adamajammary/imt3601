using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour {

	// Use this for initialization
	void Start () {
        transform.SetParent(GameObject.FindGameObjectWithTag("Player").transform);
        transform.localPosition = new Vector3(0, 250, 0);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
