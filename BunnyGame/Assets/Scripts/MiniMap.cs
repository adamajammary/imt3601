using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MiniMap : MonoBehaviour {

    public MinimapMode mode = MinimapMode.VIEW_ALL;

    private GameObject _player;
    private int _cameraHeight = 300;

    public enum MinimapMode {
        FOLLOW_PLAYER,
        VIEW_ALL
    }

	// Use this for initialization
	void Start () {

        switch (mode) {
            case MinimapMode.FOLLOW_PLAYER:
                _player = GameObject.FindGameObjectWithTag("Player");
                break;
            case MinimapMode.VIEW_ALL:
                transform.position = new Vector3(10, _cameraHeight, 5);
                GetComponent<Camera>().orthographicSize = 200;
                break;
        }
	}
	

	void Update () {
        if(mode == MinimapMode.FOLLOW_PLAYER)
            transform.position = new Vector3(_player.transform.position.x, _cameraHeight, _player.transform.position.z);
	}
}
