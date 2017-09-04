using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testController : MonoBehaviour {
    private bool up;
    private bool down;

	// Use this for initialization
	void Start () {
        Debug.Log("Im alive!");
        this.up = false;
        this.down = false;
	}
	
	public void handleInput(string input) {
        switch (input) {
            case "wDown":
                this.up = true;
                break;
            case "wUp":
                this.up = false;
                break;
            case "sDown":
                this.down = true;
                break;
            case "sUp":
                this.down = false;
                break;
            default:
                Debug.Log("Uknown input: " + input);
                break;
        }
    }

    void Update() {
        Vector3 pos = transform.position;
        if (this.up) 
            pos.y += 1;
        if (this.down)
            pos.y -= 1;
        transform.position = pos;
    }

    void OnDestroy() {
        Debug.Log("Im dead!");
    }
}
