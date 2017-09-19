using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BunnyController : NetworkBehaviour {

    private float _timer;
    private float _fireRate;

    private BunnyCommands _bunnyCommands;
    private Transform _cameraTransform;
    private CharacterController _controller;


    void Start () {
        if (!this.isLocalPlayer) { return; }

        _controller = GetComponent<CharacterController>();
        _cameraTransform = Camera.main.transform;
        //_bunnyCommands = gameObject.AddComponent<BunnyCommands>();
        _bunnyCommands = gameObject.GetComponent<BunnyCommands>();
        _timer = 0;
        _fireRate = 0.2f;

        PlayerController playerController = GetComponent<PlayerController>();
        playerController.jumpHeight = 3;
    }
	
	void Update () {
        if (!this.isLocalPlayer) { return; }

        if (Input.GetAxisRaw("Fire1") > 0)
            this.shoot();
    }

    private void shoot() {
        this._timer += Time.deltaTime;
        if (this._timer > this._fireRate) {
            this._bunnyCommands.Cmdshootpoop(this._cameraTransform.forward, this._controller.velocity);
            this._timer = 0;
        }
    }

}
