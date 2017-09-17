﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MiniMap : MonoBehaviour {

    private GameObject _player;
    private MinimapMode _mode;
    private int _cameraHeight = 300;

    public enum MinimapMode {
        FOLLOW_PLAYER,
        VIEW_ALL
    }

	// Use this for initialization
	void Start () {
        setCameraMode(MinimapMode.FOLLOW_PLAYER);
	}
	

	void Update () {
        if (_mode == MinimapMode.FOLLOW_PLAYER) {
            if (_player == null)
                setCameraMode(MinimapMode.VIEW_ALL);
            else
                transform.position = new Vector3(_player.transform.position.x, _cameraHeight, _player.transform.position.z);
        }

        if (Input.GetKeyDown(KeyCode.M)) {
            if (_mode == MinimapMode.FOLLOW_PLAYER)
                setCameraMode(MinimapMode.VIEW_ALL);
            else if (_mode == MinimapMode.VIEW_ALL)
                setCameraMode(MinimapMode.FOLLOW_PLAYER);
        }
	}

    void setCameraMode(MinimapMode mode) {
        _mode = mode;
        switch (mode) {
            case MinimapMode.FOLLOW_PLAYER:
                _player = GameObject.FindGameObjectWithTag("Player");
                GetComponent<Camera>().orthographicSize = 65;
                _player.transform.GetChild(0).localScale = new Vector3(3, 1, 3);
                break;
            case MinimapMode.VIEW_ALL:
                transform.position = new Vector3(10, _cameraHeight, 5);
                GetComponent<Camera>().orthographicSize = 200;
                _player.transform.GetChild(0).localScale = new Vector3(7, 1, 7);
                break;
        }
    }
}
