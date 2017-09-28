using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class FoxController : NetworkBehaviour {
    GameObject biteArea;
  
    private PlayerHealth _playerHealth;

    private int   _biteDamage       = 15;

    // Use this for initialization
    void Start()
    {
        if (!this.isLocalPlayer)
            return;

        this._playerHealth = this.GetComponent<PlayerHealth>();
       
        biteArea = transform.GetChild(2).gameObject;

        // Set custom attributes for class:
        PlayerController playerController = GetComponent<PlayerController>();
        playerController.runSpeed = 15;

        // Add abilities to class:
        Sprint sp = gameObject.AddComponent<Sprint>();
        sp.init(50, 1);
        playerController.abilities.Add(sp);
        Stealth st = gameObject.AddComponent<Stealth>();
        st.init(1, 0.1f);
        playerController.abilities.Add(st);
        GameObject.Find("AbilityPanel").GetComponent<AbilityPanel>().setupPanel(playerController);
    }

    // Update is called once per frame
    void Update() {
        if (!this.isLocalPlayer)
            return;

        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            CmdBite();
        }
    }

    // Biting is enabled for 1 tick after called
    private IEnumerator bite() {
        biteArea.GetComponent<BoxCollider>().enabled = true; 
        yield return 0;
        biteArea.GetComponent<BoxCollider>().enabled = false;
    }

    [Command]
    private void CmdBite() {
        if (this._playerHealth.IsDead()) { return; }

        StartCoroutine(bite());
    }

    public int getDamage() {
        return _biteDamage;
    }
}
