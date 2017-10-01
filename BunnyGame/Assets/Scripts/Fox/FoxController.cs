using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class FoxController : NetworkBehaviour {

    GameObject biteArea;

    private int  _biteDamage = 15;
    private PlayerHealth _playerHealth;



    public override void PreStartClient()
    {
        base.PreStartClient();
        NetworkAnimator netAnimator = GetComponent<NetworkAnimator>();
        for (int i = 0; i < GetComponent<Animator>().parameterCount; i++)
            netAnimator.SetParameterAutoSend(i, true);
    }

    // Use this for initialization
    void Start() {
        NetworkAnimator netAnimator = GetComponent<NetworkAnimator>();
        for (int i = 0; i < netAnimator.animator.parameterCount; i++)
            netAnimator.SetParameterAutoSend(i, true);


        biteArea = transform.GetChild(2).gameObject;
        this._playerHealth = this.GetComponent<PlayerHealth>();
        if (!this.isLocalPlayer)
            return;


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

        updateAnimator();

        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            this.bite();
        }

    }


    private void bite() {
        if (this.GetComponent<PlayerHealth>().IsDead())
            return;

        if (this.isClient)
            this.CmdBite();
        else if (this.isServer)
            this.RpcBite();

    }

    [Command]
    private void CmdBite() {
        this.RpcBite();
    }

    [ClientRpc]
    private void RpcBite() {
        StartCoroutine(this.toggleBite());
    }

    // Biting is enabled for 1 tick after called
    private IEnumerator toggleBite() {
        biteArea.GetComponent<BoxCollider>().enabled = true; 
        yield return 0;
        biteArea.GetComponent<BoxCollider>().enabled = false;
    }

    public int GetDamage() {
        return this._biteDamage;
    }

    // Updata the animator with current state
    public void updateAnimator() {
        Animator animator = GetComponent<Animator>();
        if(animator != null)
            animator.SetFloat("movespeed", GetComponent<PlayerController>().currentSpeed);
    }

    public Vector3 biteInpact()
    {
        return this.biteArea.transform.position;
    }
}
