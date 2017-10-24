﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MooseController : NetworkBehaviour{

    private GameObject ramArea;
    private int _ramDamage = 15;
    private bool _isAttackingAnim = false;


    public override void PreStartClient()
    {
        base.PreStartClient();
        NetworkAnimator netAnimator = GetComponent<NetworkAnimator>();

        for (int i = 0; i < GetComponent<Animator>().parameterCount; i++)
            netAnimator.SetParameterAutoSend(i, true);
    }

    // Use this for initialization
    void Start()
    {
        NetworkAnimator netAnimator = GetComponent<NetworkAnimator>();

        for (int i = 0; i < netAnimator.animator.parameterCount; i++)
            netAnimator.SetParameterAutoSend(i, true);

        ramArea = transform.GetChild(2).gameObject;

        if (!this.isLocalPlayer)
            return;

        // Set custom attributes for class:
        PlayerEffects pe = GetComponent<PlayerEffects>();
        pe.CmdSetAttributes(1.2f, 1.2f, 1.2f, 0.8f);

        // Add abilities to class:
        //PlayerController playerController = GetComponent<PlayerController>();
        //Sprint sp = gameObject.AddComponent<Sprint>();
        //sp.init(50, 1);
        //playerController.abilities.Add(sp);

        //Stealth st = gameObject.AddComponent<Stealth>();
        //st.init(1, 0.1f);
        //playerController.abilities.Add(st);

        //GameObject.Find("AbilityPanel").GetComponent<AbilityPanel>().setupPanel(playerController);
    }

    // Update is called once per frame
    void Update()
    {
        if (!this.isLocalPlayer)
            return;

        updateAnimator();

        if (Input.GetKeyDown(KeyCode.Mouse0))
            this.ram();
    }

    private void ram()
    {
        if (this.GetComponent<PlayerHealth>().IsDead())
            return;

        if (this.isServer)
            this.RpcRam();
        else if (this.isClient)
            this.CmdRam();
    }

    [Command]
    private void CmdRam()
    {
        this.RpcRam();
    }

    [ClientRpc]
    private void RpcRam()
    {
        StartCoroutine(this.toggleRam());
    }

    // Biting is enabled for 1 tick after called
    private IEnumerator toggleRam()
    {
        _isAttackingAnim = true;
        ramArea.GetComponent<BoxCollider>().enabled = true;
        yield return 0;
        ramArea.GetComponent<BoxCollider>().enabled = false;
        _isAttackingAnim = false;
    }

    public int GetDamage()
    {
        return this._ramDamage;
    }

    // Update the animator with current state
    public void updateAnimator()
    {
        Animator animator = GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetFloat("movespeed", GetComponent<PlayerController>().currentSpeed);
            animator.SetBool("isJumping", !GetComponent<CharacterController>().isGrounded);
            animator.SetBool("isAttacking", _isAttackingAnim);
            animator.SetFloat("height", GetComponent<PlayerController>().velocityY);
        }
    }

    public Vector3 biteImpact()
    {
        return this.ramArea.transform.position;
    }
}