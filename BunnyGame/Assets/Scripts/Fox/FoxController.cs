using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class FoxController : NetworkBehaviour {
    GameObject biteArea;

    private int _biteDamage = 15;
    private PlayerHealth _playerHealth;

    // Use this for initialization
    void Start()
    {
        biteArea = transform.GetChild(2).gameObject;
        this._playerHealth = this.GetComponent<PlayerHealth>();

        if (!this.isLocalPlayer)
            return; 

        PlayerController playerController = GetComponent<PlayerController>();
        playerController.runSpeed = 15;
    }

    // Update is called once per frame
    void Update()
    {
        if (!this.isLocalPlayer)
            return;

        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            CmdBite();
        }
    }

    // Biting is enabled for 1 tick after called
    private IEnumerator bite()
    {
        biteArea.GetComponent<BoxCollider>().enabled = true; 
        yield return 0;
        biteArea.GetComponent<BoxCollider>().enabled = false;
    }

    [Command]
    private void CmdBite()
    {
        if (this._playerHealth.IsDead()) { return; }

        StartCoroutine(bite());
    }

    public int getDamage() {
        return _biteDamage;
    }
}
