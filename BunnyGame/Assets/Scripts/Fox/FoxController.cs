using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class FoxController : NetworkBehaviour {
    GameObject biteArea;
    GameObject foxModel;

    Material[] objMaterials;
    Color alfaColor;

    private int   _biteDamage      = 15;
    private float _cooldownStealth = 30.0f;
    private float _stealthActive   = 10.0f;
    private float _transparency    = 0.1f;
    private float _notTransparent  = 1.0f;
    private float _stealthTime     = 31.0f;

    // Use this for initialization
    void Start() {
        foxModel = transform.GetChild(1).gameObject;
        biteArea = transform.GetChild(2).gameObject;

        if (!this.isLocalPlayer)
            return;

        // Set custom attributes for class:
        PlayerController playerController = GetComponent<PlayerController>();
        playerController.runSpeed = 15;

        // Add abilities to class:
        Sprint sp = gameObject.AddComponent<Sprint>();
        sp.init(50, 1);
        playerController.abilities.Add(sp);
        GameObject.Find("AbilityPanel").GetComponent<AbilityPanel>().setupPanel(playerController);
    }

    // Update is called once per frame
    void Update() {
        animate();
        if (!this.isLocalPlayer)
            return;

        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            this.bite();
        }

        this._stealthTime += Time.deltaTime;

        //The '1' key on the top of the alphanumeric keyboard
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            if (this._stealthTime >= this._cooldownStealth) {
                this.stealth();
                this._stealthTime = 0;
            }     
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

    private void stealth() {
        if (this.GetComponent<PlayerHealth>().IsDead())
            return;

        if (this.isClient)
            this.CmdStealth();
        else if (this.isServer)
            this.RpcStealth();
    }

    [Command]
    private void CmdStealth() {
        this.RpcStealth();
    }

    [ClientRpc]
    private void RpcStealth() {
        StartCoroutine(this.toggleStealth());
    }

    private IEnumerator toggleStealth() {
        this.setTransparentFox(this._transparency);
        yield return new WaitForSeconds(this._stealthActive);
        this.setTransparentFox(this._notTransparent);
    }

    private void setTransparentFox(float alpha) {
        foreach (Transform child in foxModel.transform) {
            this.objMaterials = child.gameObject.GetComponent<Renderer>().materials;
            int count = 0;

            foreach (Material mat in objMaterials) {
                alfaColor   = mat.color;
                alfaColor.a = alpha;
                objMaterials[count++].SetColor("_Color", alfaColor);
            }
        }
    }

    //[ClientRpc]
    //private void RpcSetTransparentFox() {
    //    foreach (Transform child in foxModel.transform) {
    //        objMaterials = child.gameObject.GetComponent<Renderer>().materials;
    //        int count = 0;

    //        foreach (Material mat in objMaterials) {
    //            alfaColor = mat.color;
    //            alfaColor.a = _transparency;
    //            objMaterials[count++].SetColor("_Color", alfaColor);
    //        }
    //    }
    //}

    //[ClientRpc]
    //private void RpcSetOrginalFox() {
    //    foreach (Transform child in foxModel.transform) {
    //        objMaterials = child.gameObject.GetComponent<Renderer>().materials;
    //        int count = 0;

    //        foreach (Material mat in objMaterials) {
    //            alfaColor = mat.color;
    //            alfaColor.a = _notTransparent;
    //            objMaterials[count++].SetColor("_Color", alfaColor);
    //        }
    //    }
    //}

    public void animate() {
        Animator animator = GetComponentInChildren<Animator>();
        //animator.SetFloat("forward", GetComponent<PlayerController>().currentSpeed);
    }
}
