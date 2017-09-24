﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class FoxController : NetworkBehaviour {
    GameObject biteArea;
    GameObject foxModel;

    Material[] objMaterials;

    Color alfaColor;

    private int   _biteDamage       = 15;
    private float _cooldownStealth  = 10.0f; 
    private float _transparency     = 0.1f;
    private float _notTransparent   = 1.0f;

    // Use this for initialization
    void Start()
    {
        foxModel = transform.GetChild(1).gameObject;
       
        biteArea = transform.GetChild(2).gameObject;

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

        //The '1' key on the top of the alphanumeric keyboard
        if (Input.GetKeyDown(KeyCode.Alpha1))
            CmdStealth();
         
        
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
        StartCoroutine(bite());
    }

    public int getDamage() {
        return _biteDamage;
    }

    [Command]
    private void CmdStealth()
    {
        StartCoroutine(stealth());
    }

    private IEnumerator stealth()
    {
        RpcSetTransparentFox();
        yield return new WaitForSeconds(_cooldownStealth);
        RpcSetOrginalFox();
    }

    [ClientRpc]
    private void RpcSetTransparentFox()
    {
        foreach (Transform child in foxModel.transform)
        {
            objMaterials = child.gameObject.GetComponent<Renderer>().materials;
            int count = 0;
            foreach (Material mat in objMaterials)
            {
                alfaColor = mat.color;
                alfaColor.a = _transparency;
                objMaterials[count++].SetColor("_Color", alfaColor);
            }
        }
    }

    [ClientRpc]
    private void RpcSetOrginalFox()
    {
        foreach (Transform child in foxModel.transform)
        {
            objMaterials = child.gameObject.GetComponent<Renderer>().materials;
            int count = 0;
            foreach (Material mat in objMaterials)
            {
                alfaColor = mat.color;
                alfaColor.a = _notTransparent;
                objMaterials[count++].SetColor("_Color", alfaColor);
            }
        }
    }
}
