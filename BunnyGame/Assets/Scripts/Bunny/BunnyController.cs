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
        _controller = GetComponent<CharacterController>();
        _cameraTransform = Camera.main.transform;
        _bunnyCommands = gameObject.AddComponent<BunnyCommands>();
        _timer = 0;
        _fireRate = 0.2f;

        PlayerController playerController = GetComponent<PlayerController>();
        playerController.jumpHeight = 3;
    }
	
	void Update () {
        if (Input.GetAxisRaw("Fire1") > 0 && Input.GetKey(KeyCode.Mouse1))
            this.shoot();
    }

    //private void shoot() {
    //    this._timer += Time.deltaTime;
    //    if (this._timer > this._fireRate) {
    //        this._bunnyCommands.Cmdshootpoop(this._cameraTransform.forward, this._controller.velocity);
    //        this._timer = 0;
    //    }
    //}


    private void shoot()
    {

        this._timer += Time.deltaTime;
        if (this._timer > this._fireRate)
        {

            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                Vector3 direction = hit.point - this.transform.position;
                Vector3 dirNorm = direction.normalized;
                this._bunnyCommands.Cmdshootpoop(dirNorm, this._controller.velocity);
            }
            else
            {
                Vector3 direction = ray.GetPoint(50.0f) - this.transform.position;
                Vector3 dirNorm = direction.normalized;
                this._bunnyCommands.Cmdshootpoop(dirNorm, this._controller.velocity);
            }
            this._timer = 0;
        }
    }
}
